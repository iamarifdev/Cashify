namespace Cashify.Api.Features.ActivityLogs.GetActivityLogs;

public class GetActivityLogsQuery
{
    public Guid BusinessId { get; set; }
    public Guid? CashbookId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}
