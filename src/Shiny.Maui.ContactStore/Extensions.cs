namespace Shiny.Maui.ContactStore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContactStore(this IServiceCollection services)
    {
        services.AddSingleton<IContactStore, ContactStoreImpl>();
        return services;
    }
}

public static class ContactStoreExtensions
{
    extension(IContactStore store)
    {
        public Task<PermissionStatus> CheckPermissionStatusAsync()
            => Permissions.CheckStatusAsync<ContactPermission>();

        public Task<PermissionStatus> RequestPermssionsAsync()
            => Permissions.RequestAsync<ContactPermission>();

        public async Task<IReadOnlyList<char>> GetFamilyNameFirstLetters(CancellationToken ct = default)
        {
            var contacts = await store.GetAll(ct);
            return contacts
                .Where(c => !string.IsNullOrWhiteSpace(c.FamilyName))
                .Select(c => char.ToUpperInvariant(c.FamilyName![0]))
                .Distinct()
                .Order()
                .ToList();
        }
    }
}
