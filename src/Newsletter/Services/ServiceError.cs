namespace Newsletter.Services;

/// <summary>
/// 서비스 실패 정보. API 응답의 HTTP 상태, 오류 코드, 메시지로 쓴다
/// </summary>
public record ServiceError(int StatusCode, string Code, string Message);
