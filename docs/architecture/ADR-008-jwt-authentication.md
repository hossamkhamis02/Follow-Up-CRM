# ADR-008: JWT Authentication Strategy

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM requires user authentication for all workspace-scoped operations. The application is a SPA (Angular) consuming a REST API (.NET 8). We need an authentication mechanism that:

- Works across distributed endpoints (no server-side session affinity requirement)
- Carries tenant context (WorkspaceId) alongside user identity
- Supports token refresh for long-lived sessions
- Integrates with ASP.NET Core's built-in auth infrastructure

Options considered:

1. **Cookie-based session authentication** — server-side sessions, stateful
2. **JWT (JSON Web Token) access tokens + refresh tokens** — stateless access, stateful refresh
3. **OAuth2/OIDC delegation** — delegate auth to an external provider (Auth0, Azure AD)

## Decision

We adopt **JWT access tokens with refresh tokens** as the primary authentication mechanism. ASP.NET Core Identity manages user registration, password hashing, and role storage. JWTs are issued by the API after login and contain:

- **Sub**: user ID
- **Email**: user email
- **Roles**: assigned roles (for RBAC — see ADR-009)
- **WorkspaceId**: current workspace context (for multi-tenancy — see ADR-007)
- **Exp/Iss**: standard token expiration and issuance timestamps

Token lifecycle:

- **Access token**: short-lived (15-30 minutes), used for API authorization. Stored in memory on the frontend (never in localStorage for production — use memory-only or httpOnly cookies).
- **Refresh token**: long-lived (7-30 days), stored hashed in the database. Used to obtain new access tokens without re-login. Rotation strategy: each refresh generates a new refresh token, invalidating the old one.
- **Logout**: invalidates the refresh token in the database and clears frontend storage.

Implementation:

- `System.IdentityModel.Tokens.Jwt` for token generation and validation
- ASP.NET Core `[Authorize]` attribute with JWT bearer middleware
- Custom claims added via `ClaimsPrincipal` on login
- Angular auth interceptor attaches the token to every HTTP request via `Authorization: Bearer` header

## Rationale

- **Stateless authorization**: Each API request carries all necessary context (user ID, roles, workspace) in the token. No database lookups for session validation. This scales horizontally without session store coordination.
- **Tenant context in token**: `WorkspaceId` as a JWT claim eliminates per-request tenant resolution queries. The middleware extracts it once, and EF Core filters use it for the entire request lifecycle.
- **SPA compatibility**: Angular naturally works with bearer tokens. No CORS cookie complications, no SameSite attribute issues, no session cookie conflicts across domains.
- **ASP.NET Core integration**: JWT bearer middleware is built-in, well-tested, and supports claims-based authorization natively. No custom auth pipeline needed.
- **Refresh token rotation**: Prevents token replay attacks. If a refresh token is stolen and used, the legitimate client's next refresh attempt fails (old token is invalidated), triggering re-authentication and alerting the system.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Statelessness | No session store, horizontal scaling | Token revocation requires refresh rotation, not instant |
| Token size | All claims in one payload | Larger HTTP headers (mitigated by keeping claims minimal) |
| Security | Refresh rotation prevents replay | Access token cannot be revoked mid-lifetime |
| Integration | Built-in ASP.NET Core support | Custom token storage needed on frontend |
| XSS protection | Token in memory avoids XSS exfiltration | Memory-only storage lost on tab close (mitigated by refresh) |
| Multi-device | Refresh tokens per device | Must manage multiple active refresh tokens |

## Future Evolution

- **OAuth2/OIDC delegation**: When enterprise customers require SSO (Azure AD, Google Workspace), add OIDC as an external identity provider. ASP.NET Core supports this natively — JWT issuance remains our internal flow, with external providers as upstream validators.
- **httpOnly cookie mode**: For production deployment, migrate access token storage to httpOnly cookies set by the API. This eliminates XSS token theft entirely. The Angular app never touches the token — the browser attaches it automatically.
- **Token revocation list**: For immediate access token revocation (e.g., admin suspends a user), implement a short-lived token blacklist in Redis. Check the blacklist in JWT middleware before accepting the token. This trades some statelessness for security responsiveness.
- **Two-factor authentication**: Extend ASP.NET Core Identity with TOTP (time-based one-time password) for high-security workspace roles. The JWT claims include a `2fa_verified` flag that gates sensitive operations.