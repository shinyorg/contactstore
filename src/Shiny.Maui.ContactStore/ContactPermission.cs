#if IOS
using Contacts;
#endif

namespace Shiny.Maui.ContactStore;

public class ContactPermission : Permissions.BasePlatformPermission
{
    #if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        => [
            (Android.Manifest.Permission.WriteContacts, true),
            (Android.Manifest.Permission.ReadContacts, true)
        ];

    public override async Task<PermissionStatus> RequestAsync()
    {
        var status = await base.RequestAsync();
        if (status == PermissionStatus.Granted)
            return status;

        return HasEitherPermission() ? PermissionStatus.Limited : PermissionStatus.Denied;
    }

    public override async Task<PermissionStatus> CheckStatusAsync()
    {
        var status = await base.CheckStatusAsync();
        if (status == PermissionStatus.Granted)
            return status;

        return HasEitherPermission() ? PermissionStatus.Limited : PermissionStatus.Denied;
    }

    static bool HasEitherPermission()
    {
        var context = Android.App.Application.Context;
        var readGranted = context.CheckSelfPermission(Android.Manifest.Permission.ReadContacts) == Android.Content.PM.Permission.Granted;
        var writeGranted = context.CheckSelfPermission(Android.Manifest.Permission.WriteContacts) == Android.Content.PM.Permission.Granted;
        return readGranted || writeGranted;
    }


    #elif IOS
    public override Task<PermissionStatus> RequestAsync()
        => CheckStatusAsync();

    public override Task<PermissionStatus> CheckStatusAsync()
    {
        var status = CNContactStore.GetAuthorizationStatus(CNEntityType.Contacts);
        return Task.FromResult(status switch
        {
            CNAuthorizationStatus.Authorized => PermissionStatus.Granted,
            CNAuthorizationStatus.Denied => PermissionStatus.Denied,
            CNAuthorizationStatus.Restricted => PermissionStatus.Restricted,
            _ => PermissionStatus.Unknown
        });
    }

    #endif
}
