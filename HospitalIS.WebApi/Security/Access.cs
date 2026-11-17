using System.Security.Claims;

namespace HospitalIS.WebApi.Security;

public static class Roles
{
    public const string Admin = "Administrator";
    public const string Doctor = "Doctor";
    public const string Patient = "Patient";
}

public static class PrincipalExtensions
{
    public static long UserId(this ClaimsPrincipal p)
        => long.TryParse(p.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static bool IsAdmin(this ClaimsPrincipal p) => p.IsInRole(Roles.Admin);
    public static bool IsDoctor(this ClaimsPrincipal p) => p.IsInRole(Roles.Doctor);
    public static bool IsPatient(this ClaimsPrincipal p) => p.IsInRole(Roles.Patient);
}