namespace OrlandoUp.Application;

/// <summary>Named policies, so that a folder convention and a page attribute cannot drift apart.</summary>
public static class AuthorizationPolicies
{
    /// <summary>Either staff role: the whole administration sits behind it.</summary>
    public const string Staff = "StaffOnly";
}
