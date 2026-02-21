using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.MacOS;

public static class EssentialsExtensions
{
    public static MauiAppBuilder AddMacOSEssentials(this MauiAppBuilder builder)
    {
        builder.Services.TryAddSingleton<IAppInfo, AppInfoImplementation>();
        builder.Services.TryAddSingleton<IDeviceInfo, DeviceInfoImplementation>();
        builder.Services.TryAddSingleton<IConnectivity, ConnectivityImplementation>();
        builder.Services.TryAddSingleton<IBattery, BatteryImplementation>();
        builder.Services.TryAddSingleton<IDeviceDisplay, DeviceDisplayImplementation>();
        builder.Services.TryAddSingleton<IFileSystem, FileSystemImplementation>();
        builder.Services.TryAddSingleton<IPreferences, PreferencesImplementation>();
        builder.Services.TryAddSingleton<ISecureStorage, SecureStorageImplementation>();
        builder.Services.TryAddSingleton<IFilePicker, FilePickerImplementation>();
        builder.Services.TryAddSingleton<IMediaPicker, MediaPickerImplementation>();
        builder.Services.TryAddSingleton<ITextToSpeech, TextToSpeechImplementation>();
        builder.Services.TryAddSingleton<IClipboard, ClipboardImplementation>();
        builder.Services.TryAddSingleton<IBrowser, BrowserImplementation>();
        builder.Services.TryAddSingleton<IShare, ShareImplementation>();
        builder.Services.TryAddSingleton<ILauncher, LauncherImplementation>();
        builder.Services.TryAddSingleton<IMap, MapImplementation>();
        builder.Services.TryAddSingleton<IVibration, VibrationImplementation>();
        builder.Services.TryAddSingleton<ISemanticScreenReader, SemanticScreenReaderImplementation>();
        builder.Services.TryAddSingleton<IGeolocation, GeolocationImplementation>();

        SetEssentialsDefaults();

        return builder;
    }

    private static void SetEssentialsDefaults()
    {
        SetStaticField(typeof(AppInfo), "currentImplementation", new AppInfoImplementation());
        SetStaticField(typeof(DeviceInfo), "currentImplementation", new DeviceInfoImplementation());
        SetStaticField(typeof(Connectivity), "currentImplementation", new ConnectivityImplementation());
        SetStaticField(typeof(Battery), "defaultImplementation", new BatteryImplementation());
        SetStaticField(typeof(DeviceDisplay), "currentImplementation", new DeviceDisplayImplementation());
        SetStaticField(typeof(FileSystem), "currentImplementation", new FileSystemImplementation());
        SetStaticField(typeof(Preferences), "defaultImplementation", new PreferencesImplementation());
        SetStaticField(typeof(SecureStorage), "defaultImplementation", new SecureStorageImplementation());
        SetStaticField(typeof(FilePicker), "defaultImplementation", new FilePickerImplementation());
        SetStaticField(typeof(MediaPicker), "defaultImplementation", new MediaPickerImplementation());
        SetStaticField(typeof(TextToSpeech), "defaultImplementation", new TextToSpeechImplementation());
        SetStaticField(typeof(Clipboard), "defaultImplementation", new ClipboardImplementation());
        SetStaticField(typeof(Browser), "defaultImplementation", new BrowserImplementation());
        SetStaticField(typeof(Share), "defaultImplementation", new ShareImplementation());
        SetStaticField(typeof(Launcher), "defaultImplementation", new LauncherImplementation());
        SetStaticField(typeof(ApplicationModel.Map), "defaultImplementation", new MapImplementation());
        SetStaticField(typeof(Vibration), "defaultImplementation", new VibrationImplementation());
        SetStaticField(typeof(SemanticScreenReader), "defaultImplementation", new SemanticScreenReaderImplementation());
        SetStaticField(typeof(Geolocation), "defaultImplementation", new GeolocationImplementation());
    }

    static void SetStaticField(Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, value);
    }
}
