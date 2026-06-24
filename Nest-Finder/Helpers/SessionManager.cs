using NestFinder.Models;

namespace NestFinder.Helpers;

public static class SessionManager
{
    public static User? CurrentUser { get; set; }
    public static bool IsAdmin => CurrentUser?.Role == "Admin";
    public static bool IsCustomer => CurrentUser?.Role == "Customer";
    public static void Logout() { CurrentUser = null; }
}
