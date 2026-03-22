namespace Shiny.Mobile.ContactStore;

public interface IContactStore
{
    /// <summary>
    /// Requests permission to access the device's contacts.
    /// Returns true if permission was granted, false otherwise.
    /// </summary>
    Task<bool> RequestPermission(CancellationToken ct = default);

    /// <summary>
    /// Retrieves all contacts from the device.
    /// </summary>
    Task<IReadOnlyList<Contact>> GetAll(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single contact by its platform identifier.
    /// </summary>
    Task<Contact?> GetById(string contactId, CancellationToken ct = default);

    /// <summary>
    /// Returns a LINQ-queryable source of contacts.
    /// Supports .Where() with .Contains(), .StartsWith(), .Equals() on string properties.
    /// Predicates are translated to native queries where possible, with in-memory fallback.
    /// </summary>
    IQueryable<Contact> Query();

    /// <summary>
    /// Creates a new contact and returns the platform-assigned identifier.
    /// </summary>
    Task<string> Create(Contact contact, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing contact. The contact must have a valid Id.
    /// </summary>
    Task Update(Contact contact, CancellationToken ct = default);

    /// <summary>
    /// Deletes the contact with the specified identifier.
    /// </summary>
    Task Delete(string contactId, CancellationToken ct = default);
}
