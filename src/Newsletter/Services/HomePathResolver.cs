using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 로그인 후 권한별 첫 화면 경로 (D-048)
/// </summary>
public static class HomePathResolver
{
    public const string AdminHomePath = "/Admin/Send";
    public const string UserHomePath = "/History";

    public static string GetHomePath(string? role)
    {
        return role == UserRole.Admin ? AdminHomePath : UserHomePath;
    }
}
