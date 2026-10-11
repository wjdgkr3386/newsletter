namespace Newsletter.Services;

/// <summary>
/// 공휴일 동기화 결과. 실패하면 1시간 뒤 다시 시도한다 (D-080)
/// </summary>
/// <param name="IsSuccess">API로 받기에 성공했으면 true</param>
/// <param name="AddedCount">DB에 새로 추가한 날짜 수 (이미 있는 날짜는 세지 않음)</param>
public record HolidaySyncResult(bool IsSuccess, int AddedCount)
{
    public static HolidaySyncResult Failure() => new(false, 0);
}
