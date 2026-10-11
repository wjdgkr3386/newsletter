namespace Newsletter.Services;

/// <summary>
/// 서비스 처리 결과. 성공이면 Data, 실패면 Error를 담는다
/// </summary>
public sealed class ServiceResult<T>
{
    private ServiceResult(T? data, ServiceError? error)
    {
        Data = data;
        Error = error;
    }

    public T? Data { get; }

    public ServiceError? Error { get; }

    public bool IsSuccess => Error is null;

    public static ServiceResult<T> Success(T data) => new(data, null);

    public static ServiceResult<T> Failure(int statusCode, string code, string message) => new(default, new ServiceError(statusCode, code, message));

    public static ServiceResult<T> Failure(ServiceError error) => new(default, error);
}
