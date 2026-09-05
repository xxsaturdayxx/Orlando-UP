namespace OrlandoUp.Application;

/// <summary>The two staff roles (D8/01). There is no customer account in this release.</summary>
public static class Roles
{
    public const string Admin = "Admin";

    public const string Staff = "Staff";

    public static readonly string[] All = [Admin, Staff];
}
