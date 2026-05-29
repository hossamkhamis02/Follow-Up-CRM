# ADR-012: Responsive SaaS UI Strategy

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM is a SaaS product that users access from various devices — desktops in the office, tablets in meetings, phones on the go. The primary use case is desktop-heavy (customer management, follow-up scheduling, dashboard analytics), but mobile access is critical for field sales reps checking follow-ups and receiving notifications.

We need a UI strategy that:

- Provides full functionality on desktop
- Offers usable mobile experience for core workflows (view customers, check follow-ups, quick actions)
- Maintains a single codebase to reduce maintenance cost
- Supports SaaS branding customization per workspace (logo, colors, name)

Options considered:

1. **Separate mobile app** — native iOS/Android app for mobile, web app for desktop. Maximum mobile UX, but doubles development effort.
2. **Responsive web application** — single Angular SPA with responsive layouts. Desktop-first design with mobile-adapted layouts.
3. **Progressive Web App (PWA)** — responsive web with service worker, offline caching, push notifications, and install-to-home-screen.

## Decision

We adopt **Responsive web application** with Angular 19, built as a **desktop-first responsive SPA** that adapts gracefully to tablet and phone viewports. PWA capabilities (offline caching, push notifications) will be added incrementally as Phase 2 features.

UI architecture:

- **Angular 19 standalone components** — no NgModules, modern Angular patterns
- **Feature-based lazy loading** — each module (customers, follow-ups, dashboard) loads on demand via `loadChildren()` routes
- **CSS responsive strategy** — desktop-first with `max-width` breakpoints for tablet (1024px) and mobile (768px). Tailwind CSS utility classes for layout, custom SCSS for theme variables.
- **App shell pattern** — `AppShellComponent` provides the sidebar + header layout for authenticated routes. Identity pages (login/register) bypass the shell.
- **SaaS customization** — workspace theme (primary color, logo, display name) loaded on login, stored in `AuthService`, applied via CSS custom properties (`--primary-color`, `--workspace-logo`)

Mobile-specific adaptations:

- Sidebar collapses to bottom navigation on mobile
- Data tables switch to card-based layouts below tablet breakpoint
- Forms stack vertically on mobile
- Quick-action floating button for primary operations (add follow-up, create customer)

## Rationale

- **Single codebase**: One Angular project serves all devices. No separate mobile team, no API duplication, no sync issues between web and mobile versions.
- **Desktop-first reality**: CRM users spend 80%+ of time on desktop managing data. Mobile is supplementary (notifications, quick views). A desktop-first responsive approach invests effort where usage is highest.
- **Angular standalone components**: Eliminates NgModule ceremony. Each component, route, and guard is self-contained. This matches the vertical slice backend — features are independent units.
- **Lazy loading alignment**: Frontend modules load on demand, matching backend module structure. The customer feature loads only when the user navigates to `/customers`. This keeps the initial bundle small.
- **PWA as incremental upgrade**: PWA requires service worker configuration, offline data strategy, and push notification infrastructure. Starting with responsive web gives us a working product faster; PWA features are added when the core product is stable.
- **Tailwind + custom properties**: Utility-first CSS provides responsive layout primitives without writing custom media queries for every component. CSS custom properties enable per-workspace branding without rebuilding stylesheets.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Development cost | Single codebase, one team | Mobile UX limited by responsive adaptation |
| Performance | Lazy-loaded modules | Initial shell load includes layout components |
| Offline support | Future PWA upgrade path | No offline capability initially |
| Native features | PWA provides some (push, install) | No native APIs (contacts, calendar deep integration) |
| Branding | CSS custom properties per workspace | Custom properties limited — cannot change layout structure |
| Accessibility | Web standards compliant | Complex responsive components need extra a11y testing |

## Future Evolution

- **PWA Phase 2**: Add service worker with offline caching for follow-ups data. Enable push notifications for follow-up reminders. Add `manifest.json` for home screen installation. This targets field sales reps who need mobile notifications.
- **Mobile-first feature slices**: For workflows primarily used on mobile (e.g., "check today's follow-ups", "quick add a note"), create dedicated mobile-optimized components with touch-first interactions. These are loaded via separate routes under the same feature module.
- **Native companion app**: If mobile usage grows beyond what responsive web can deliver, build a lightweight native companion (React Native or Flutter) focused on field workflows. The web app remains the primary interface; the native app is a targeted supplement, not a replacement.
- **Workspace theming engine**: Expand CSS custom properties to a full theme engine. Store theme configuration in the database. Generate theme CSS dynamically on workspace setup. Support light/dark mode per user preference.
- **Component library**: Extract reusable UI components (data tables with responsive modes, form controls, card layouts) into a shared Angular library. This standardizes responsive behavior across all feature modules.