using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.TvOS;

public static class EssentialsExtensions
{
    public static void UseTvOSEssentials()
    {
        SetStaticField(typeof(AppInfo), "currentImplementation", new AppInfoImplementation());
        SetStaticField(typeof(DeviceInfo), "currentImplementation", new DeviceInfoImplementation());
        SetStaticField(typeof(Connectivity), "currentImplementation", new ConnectivityImplementation());
        SetStaticField(typeof(DeviceDisplay), "currentImplementation", new DeviceDisplayImplementation());
        SetStaticField(typeof(FileSystem), "currentImplementation", new FileSystemImplementation());
        SetStaticField(typeof(Preferences), "defaultImplementation", new PreferencesImplementation());
        SetStaticField(typeof(SecureStorage), "defaultImplementation", new SecureStorageImplementation());
        SetStaticField(typeof(TextToSpeech), "defaultImplementation", new TextToSpeechImplementation());
        SetStaticField(typeof(Clipboard), "defaultImplementation", new ClipboardImplementation());
    }

    static void SetStaticField(Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, value);
    }
}
