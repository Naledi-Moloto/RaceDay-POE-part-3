using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

// Small helpers so we can write Session.IsLoggedIn() instead of repeating key names
public static class UserSessionExtensions
{
    public static void SaveLogin(this ISession session, AuthResponse auth)
    {
        session.SetString(SessionKeys.Token, auth.Token);
        session.SetString(SessionKeys.Role, auth.Role);
        session.SetString(SessionKeys.FullName, auth.FullName);
        session.SetInt32(SessionKeys.UserId, auth.UserID);
    }

    public static bool IsLoggedIn(this ISession session)
    {
        return !string.IsNullOrEmpty(session.GetString(SessionKeys.Token));
    }

    public static string? GetRole(this ISession session)
    {
        return session.GetString(SessionKeys.Role);
    }

    public static string GetFullName(this ISession session)
    {
        return session.GetString(SessionKeys.FullName) ?? string.Empty;
    }

    public static int GetUserId(this ISession session)
    {
        return session.GetInt32(SessionKeys.UserId) ?? 0;
    }
}