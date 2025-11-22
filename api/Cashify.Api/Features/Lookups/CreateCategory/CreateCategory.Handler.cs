using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Lookups.CreateCategory;

public class CreateCategoryHandler
{
    private readonly AppDbContext _dbContext;

    public CreateCategoryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> Handle(CreateCategoryCommand command, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == command.BusinessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            BusinessId = command.BusinessId,
            Name = command.Name,
            Type = command.Type,
            CreatedByUserId = userId
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
