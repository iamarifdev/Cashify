namespace Cashify.Api.Features.Cashbooks.CreateCashbook;

public class CreateCashbookCommand
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
}

