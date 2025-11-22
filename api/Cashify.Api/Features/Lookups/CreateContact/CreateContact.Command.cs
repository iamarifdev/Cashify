namespace Cashify.Api.Features.Lookups.CreateContact;

public class CreateContactCommand
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "customer";
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

