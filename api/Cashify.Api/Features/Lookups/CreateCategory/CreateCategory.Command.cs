namespace Cashify.Api.Features.Lookups.CreateCategory;

public class CreateCategoryCommand
{
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "expense";
}

