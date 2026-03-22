using Microsoft.Extensions.DependencyInjection;

namespace Shiny.Mobile.ContactStore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContactStore(this IServiceCollection services)
    {
        services.AddSingleton<IContactStore, ContactStoreImpl>();
        return services;
    }
}
