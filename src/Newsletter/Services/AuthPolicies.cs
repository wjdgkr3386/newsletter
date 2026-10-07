namespace Newsletter.Services;

/// <summary>
/// 권한 정책 이름 (D-017: 일반 사용자는 히스토리만, 관리자는 모든 화면)
/// </summary>
public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
}
