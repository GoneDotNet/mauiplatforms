// TODO: DispatchProxy is not supported on tvOS (AOT - no dynamic code generation)
// Waiting on https://github.com/dotnet/maui/issues/XXXXX to make IAlertManagerSubscription public
// Once public, remove DispatchProxy usage and implement the interface directly

#if false
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Platform.TvOS;

#pragma warning disable IL2026, IL2060, IL2080, IL2111 // Reflection required for internal IAlertManagerSubscription
#pragma warning disable CA1422 // Obsolete UIApplication.KeyWindow/Windows - tvOS compatibility

public class AlertManagerSubscription : DispatchProxy
{
    static readonly Type? AlertManagerType = typeof(Window).Assembly
        .GetType("Microsoft.Maui.Controls.Platform.AlertManager");

    static readonly Type? IAlertManagerSubscriptionType = AlertManagerType?
        .GetNestedType("IAlertManagerSubscription", BindingFlags.Public | BindingFlags.NonPublic);

    public static void Register(IServiceCollection services)
    {
        if (IAlertManagerSubscriptionType == null)
            return;

        var proxyType = typeof(AlertManagerSubscription<>).MakeGenericType(IAlertManagerSubscriptionType);
        var createMethod = typeof(DispatchProxy)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Create" && m.GetGenericArguments().Length == 2)
            .MakeGenericMethod(IAlertManagerSubscriptionType, proxyType);

        var proxy = createMethod.Invoke(null, null)!;
        services.AddSingleton(IAlertManagerSubscriptionType, proxy);
    }

    internal static void HandleInvoke(MethodInfo? method, object?[]? args)
    {
        if (method == null || args == null)
            return;

        switch (method.Name)
        {
            case "OnAlertRequested":
                OnAlertRequested(args[0] as Page, args[1] as AlertArguments);
                break;
            case "OnPromptRequested":
                OnPromptRequested(args[0] as Page, args[1] as PromptArguments);
                break;
            case "OnActionSheetRequested":
                OnActionSheetRequested(args[0] as Page, args[1] as ActionSheetArguments);
                break;
        }
    }

    static void OnAlertRequested(Page? sender, AlertArguments? arguments)
    {
        if (arguments == null)
            return;

        var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);

        if (arguments.Cancel != null)
            alert.AddAction(UIAlertAction.Create(arguments.Cancel, UIAlertActionStyle.Cancel, _ => arguments.SetResult(false)));

        if (arguments.Accept != null)
            alert.AddAction(UIAlertAction.Create(arguments.Accept, UIAlertActionStyle.Default, _ => arguments.SetResult(true)));

        PresentAlert(alert);
    }

    static void OnPromptRequested(Page? sender, PromptArguments? arguments)
    {
        if (arguments == null)
            return;

        var alert = UIAlertController.Create(arguments.Title, arguments.Message, UIAlertControllerStyle.Alert);
        alert.AddTextField(tf =>
        {
            tf.Placeholder = arguments.Placeholder;
            tf.Text = arguments.InitialValue;
            if (arguments.MaxLength > -1)
            {
                tf.ShouldChangeCharacters = (field, range, replacement) =>
                    field.Text!.Length + replacement.Length - (int)range.Length <= arguments.MaxLength;
            }
        });

        alert.AddAction(UIAlertAction.Create(arguments.Cancel, UIAlertActionStyle.Cancel, _ => arguments.SetResult(null)));
        alert.AddAction(UIAlertAction.Create(arguments.Accept, UIAlertActionStyle.Default, _ => arguments.SetResult(alert.TextFields![0].Text)));

        PresentAlert(alert);
    }

    static void OnActionSheetRequested(Page? sender, ActionSheetArguments? arguments)
    {
        if (arguments == null)
            return;

        var alert = UIAlertController.Create(arguments.Title, null, UIAlertControllerStyle.ActionSheet);

        if (arguments.Cancel != null)
            alert.AddAction(UIAlertAction.Create(arguments.Cancel, UIAlertActionStyle.Cancel, _ => arguments.SetResult(arguments.Cancel)));

        if (arguments.Destruction != null)
            alert.AddAction(UIAlertAction.Create(arguments.Destruction, UIAlertActionStyle.Destructive, _ => arguments.SetResult(arguments.Destruction)));

        foreach (var button in arguments.Buttons)
        {
            if (button == null) continue;
            var label = button;
            alert.AddAction(UIAlertAction.Create(label, UIAlertActionStyle.Default, _ => arguments.SetResult(label)));
        }

        PresentAlert(alert);
    }

    static void PresentAlert(UIAlertController alert)
    {
        var vc = GetTopViewController();
        vc?.PresentViewController(alert, true, null);
    }

    static UIViewController? GetTopViewController()
    {
        var window = UIApplication.SharedApplication.KeyWindow
            ?? UIApplication.SharedApplication.Windows.FirstOrDefault();

        var vc = window?.RootViewController;
        while (vc?.PresentedViewController != null)
            vc = vc.PresentedViewController;

        return vc;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => null;
}

public class AlertManagerSubscription<T> : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        AlertManagerSubscription.HandleInvoke(targetMethod, args);
        return null;
    }
}
#endif
