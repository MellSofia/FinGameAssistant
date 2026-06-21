public static class UserSession
{
    public static int CurrentUserId { get; set; }
    public static string? CurrentUsername { get; set; }

    public static bool IsUserLoggedIn => CurrentUserId > 0;
}