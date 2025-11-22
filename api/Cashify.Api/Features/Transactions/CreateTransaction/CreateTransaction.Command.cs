namespace Cashify.Api.Features.Transactions.CreateTransaction;

public class CreateTransactionCommand
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = "expense";
    public Guid? CategoryId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? InlineCategoryName { get; set; }
    public string? InlineContactName { get; set; }
    public string? InlinePaymentMethodName { get; set; }
}

