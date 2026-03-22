using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using Shiny.Mobile.ContactStore;
using Contact = Shiny.Mobile.ContactStore.Contact;

namespace Sample.Maui;

public partial class ContactListViewModel(
    IContactStore contactStore,
    INavigator navigator,
    IDialogs dialogs
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    string searchText = string.Empty;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoPermission))]
    bool hasPermission = true;

    public bool HasNoPermission => !HasPermission;

    public ObservableCollection<Contact> Contacts { get; } = [];

    [RelayCommand]
    async Task LoadContacts()
    {
        try
        {
            var permission = await contactStore.RequestPermission();
            if (!permission)
            {
                await dialogs.Alert("FAIL", "Permission Not Granted", "OK");
                return;
            }
            IsRefreshing = true;
            HasPermission = true;

            IReadOnlyList<Contact> results;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await contactStore.GetAll();
            }
            else
            {
                var search = SearchText.Trim();
                results = contactStore
                    .Query()
                    .Where(c =>
                        c.GivenName!.Contains(search) ||
                        c.FamilyName!.Contains(search) ||
                        c.Phones.Any(p => p.Number.Contains(search)) ||
                        c.Emails.Any(e => e.Address.Contains(search)))
                    .ToList()
                    .AsReadOnly();
            }

            Contacts.Clear();
            foreach (var contact in results)
                Contacts.Add(contact);
        }
        catch (UnauthorizedAccessException)
        {
            HasPermission = false;
        }
        catch (Exception ex)
        {
            await dialogs.Alert("Error", ex.ToString(), "OK");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    Task Search() => LoadContacts();

    [RelayCommand]
    Task AddContact() => navigator.NavigateTo<ContactEditViewModel>();

    [RelayCommand]
    async Task ViewContact(Contact contact)
    {
        if (contact?.Id == null) return;
        
        await navigator.NavigateTo<ContactDetailViewModel>(vm => vm.ContactId = contact.Id);
    }

    [RelayCommand]
    async Task DeleteContact(Contact contact)
    {
        if (contact?.Id == null) return;

        var confirm = await dialogs.Confirm(
            "Delete Contact",
            $"Delete {contact.DisplayName}?",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await contactStore.Delete(contact.Id);
            Contacts.Remove(contact);
        }
        catch (Exception ex)
        {
            await dialogs.Alert("Error", ex.ToString(), "OK");
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        // Auto-search as user types (debounced by UI)
        SearchCommand.Execute(null);
    }

    public void OnAppearing() => _ = this.LoadContacts();
    public void OnDisappearing() { }
}
