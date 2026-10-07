using Newsletter.Models.Entities;
using Newsletter.Services;

namespace Newsletter.Tests.Services;

/// <summary>
/// 로그인 후 권한별 첫 화면 (D-048) 테스트
/// </summary>
public class HomePathResolverTests
{
    [Theory]
    [InlineData(UserRole.Admin, "/Admin/Send")]
    [InlineData(UserRole.User, "/History")]
    [InlineData(null, "/History")] // 권한 정보가 없으면 가장 좁은 권한의 화면
    [InlineData("admin", "/History")] // 대소문자가 다르면 관리자로 보지 않는다
    public void GetHomePath_ByRole(string? role, string expectedPath)
    {
        Assert.Equal(expectedPath, HomePathResolver.GetHomePath(role));
    }
}
