namespace FollowUpCrm.Api.Authentication;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CrmUser = "CrmUser";
    public const string SalesRepOrAdmin = "SalesRepOrAdmin";
}
