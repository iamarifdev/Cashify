namespace Cashify.Api.Features.Transactions.UpdateTransaction;

public class UpdateTransactionCommand
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = "expense";
    public Guid? CategoryId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}

