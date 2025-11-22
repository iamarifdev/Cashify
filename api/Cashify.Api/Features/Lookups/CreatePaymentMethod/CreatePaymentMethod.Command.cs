namespace Cashify.Api.Features.Lookups.CreatePaymentMethod;

public class CreatePaymentMethodCommand
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
}

