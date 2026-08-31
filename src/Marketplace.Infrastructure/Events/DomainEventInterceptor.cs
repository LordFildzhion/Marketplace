using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Infrastructure.Events;

public class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var domainEvents = context.ChangeTracker.Entries<IAggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var saveResult = await base.SavingChangesAsync(eventData, result, cancellationToken);

        if (domainEvents.Any())
        {
            var dispatcher = _serviceProvider.GetRequiredService<IDomainEventDispatcher>();
            await dispatcher.DispatchAsync(domainEvents);

            foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
                entry.Entity.ClearDomainEvents();
        }

        return saveResult;
    }
}
