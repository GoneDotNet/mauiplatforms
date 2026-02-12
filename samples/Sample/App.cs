using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Sample;

public class App : Microsoft.Maui.Controls.Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // To view the controls demo, uncomment the line below and comment the MainPage line
        // return new Window(new ControlsDemo());
        return new Window(new NavigationPage(new MainPage()));
    }
}
