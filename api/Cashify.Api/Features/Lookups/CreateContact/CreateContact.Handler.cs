using Cashify.Api.Database;
using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashify.Api.Features.Lookups.CreateContact;

public class CreateContactHandler
{
    private readonly AppDbContext _dbContext;

    public CreateContactHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> Handle(CreateContactCommand command, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = await _dbContext.BusinessMembers.AnyAsync(x => x.BusinessId == command.BusinessId && x.UserId == userId, cancellationToken);
        if (!isMember)
        {
            return null;
        }

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            BusinessId = command.BusinessId,
            Name = command.Name,
            Type = command.Type,
            Phone = command.Phone,
            Email = command.Email,
            CreatedByUserId = userId
        };

        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return contact.Id;
    }
}
