# Shiny.Mobile.ContactStore

A cross-platform .NET library for accessing device contacts on Android and iOS. Provides full CRUD operations, LINQ query support, and dependency injection integration.

## Platforms

| Platform | Minimum Version |
|----------|----------------|
| Android  | API 24         |
| iOS      | 15.0           |

## Setup

### Install

```
dotnet add package Shiny.Mobile.ContactStore
```

### Registration

```csharp
builder.Services.AddContactStore();
```

### Platform Permissions

**Android** — Add to `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.READ_CONTACTS" />
<uses-permission android:name="android.permission.WRITE_CONTACTS" />
```

**iOS** — Add to `Info.plist`:

```xml
<key>NSContactsUsageDescription</key>
<string>This app needs access to your contacts.</string>
```

#### iOS Notes & Relations Entitlement

Reading and writing the `Note` and `Relationships` properties on iOS requires the `com.apple.developer.contacts.notes` entitlement. The library automatically detects whether this entitlement is present at runtime. If absent, `Note` will return `null` and `Relationships` will be empty — no configuration or error handling needed on your part.

To enable notes and relations, add an `Entitlements.plist` with:

```xml
<key>com.apple.developer.contacts.notes</key>
<true/>
```

> **Note:** This entitlement requires approval from Apple before it can be used in production apps.

## Usage

Inject `IContactStore` into your class:

```csharp
public class MyService(IContactStore contactStore)
{
}
```

### Request Permission

```csharp
bool granted = await contactStore.RequestPermission();
```

### Get All Contacts

```csharp
IReadOnlyList<Contact> contacts = await contactStore.GetAll();
```

### Get Contact by ID

```csharp
Contact? contact = await contactStore.GetById("some-id");
```

### Query with LINQ

The library supports LINQ queries with native translation where possible and in-memory fallback for unsupported filters.

```csharp
// Filter by name
var results = contactStore.Query()
    .Where(c => c.GivenName.Contains("John"))
    .ToList();

// Filter by phone number
var results = contactStore.Query()
    .Where(c => c.Phones.Any(p => p.Number.Contains("555")))
    .ToList();

// Filter by email
var results = contactStore.Query()
    .Where(c => c.Emails.Any(e => e.Address.Contains("@example.com")))
    .ToList();

// Combine filters
var results = contactStore.Query()
    .Where(c => c.GivenName.StartsWith("J") && c.FamilyName.Contains("Smith"))
    .ToList();

// Paging
var page = contactStore.Query()
    .Where(c => c.FamilyName.StartsWith("A"))
    .Skip(10)
    .Take(20)
    .ToList();
```

**Supported query operations:** `Contains`, `StartsWith`, `EndsWith`, `Equals`

**Filterable properties:** `GivenName`, `FamilyName`, `MiddleName`, `NamePrefix`, `NameSuffix`, `Nickname`, `DisplayName`, `Note`

**Filterable collections:** `Phones` (by `Number`), `Emails` (by `Address`)

### Create a Contact

```csharp
var contact = new Contact
{
    GivenName = "John",
    FamilyName = "Doe",
    Note = "Met at conference"
};
contact.Phones.Add(new ContactPhone("555-1234", PhoneType.Mobile));
contact.Emails.Add(new ContactEmail("john@example.com", EmailType.Work));

string id = await contactStore.Create(contact);
```

### Update a Contact

```csharp
var contact = await contactStore.GetById(id);
contact.GivenName = "Jane";
await contactStore.Update(contact);
```

### Delete a Contact

```csharp
await contactStore.Delete(contactId);
```

## Models

### Contact

| Property       | Type                        | Description                                    |
|----------------|-----------------------------|------------------------------------------------|
| Id             | `string?`                   | Platform-assigned identifier                   |
| NamePrefix     | `string?`                   | e.g. "Mr.", "Dr."                              |
| GivenName      | `string?`                   | First name                                     |
| MiddleName     | `string?`                   | Middle name                                    |
| FamilyName     | `string?`                   | Last name                                      |
| NameSuffix     | `string?`                   | e.g. "Jr.", "III"                              |
| Nickname       | `string?`                   | Nickname                                       |
| DisplayName    | `string?`                   | Auto-generated from name parts if not set      |
| Note           | `string?`                   | iOS requires entitlement (see above)           |
| Organization   | `ContactOrganization?`      | Company, Title, Department                     |
| Photo          | `byte[]?`                   | Full-size photo                                |
| Thumbnail      | `byte[]?`                   | Thumbnail photo                                |
| Phones         | `List<ContactPhone>`        | Phone numbers                                  |
| Emails         | `List<ContactEmail>`        | Email addresses                                |
| Addresses      | `List<ContactAddress>`      | Postal addresses                               |
| Dates          | `List<ContactDate>`         | Birthdays, anniversaries, etc.                 |
| Relationships  | `List<ContactRelationship>` | iOS requires entitlement (see above)           |
| Websites       | `List<ContactWebsite>`      | URLs                                           |

### Enums

**PhoneType:** Home, Mobile, Work, FaxWork, FaxHome, Pager, Other, Custom

**EmailType:** Home, Work, Other, Custom

**AddressType:** Home, Work, Other, Custom

**ContactDateType:** Birthday, Anniversary, Other, Custom

**RelationshipType:** Father, Mother, Parent, Brother, Sister, Child, Friend, Spouse, Partner, Assistant, Manager, Other, Custom
