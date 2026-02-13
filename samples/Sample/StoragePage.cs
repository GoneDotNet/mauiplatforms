using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Sample;

public class StoragePage : ContentPage
{
    const string PrefKey = "test_pref";
    const string SecureKey = "test_secret";

    Label _prefValueLabel;
    Label _secureValueLabel;
    Label _statusLabel;
    Entry _prefEntry;
    Entry _secureEntry;
    int _counter;

    public StoragePage()
    {
        var layout = new VerticalStackLayout { Padding = 20, Spacing = 10 };

        _statusLabel = new Label { FontSize = 14, TextColor = Color.FromArgb("#FFD93D"), Text = "Ready" };
        layout.Children.Add(_statusLabel);

        // Preferences section
        layout.Children.Add(CreateHeader("Preferences"));

        _prefValueLabel = new Label
        {
            FontSize = 16, TextColor = Colors.White,
            Text = $"Stored: {TryGet(() => Preferences.Get(PrefKey, "(not set)"))}"
        };
        layout.Children.Add(_prefValueLabel);

        _prefEntry = new Entry { Placeholder = "Enter a value", TextColor = Colors.White };
        layout.Children.Add(_prefEntry);

        var prefSaveBtn = new Button { Text = "Save Preference" };
        prefSaveBtn.Clicked += OnSavePref;
        layout.Children.Add(prefSaveBtn);

        var prefCounterBtn = new Button { Text = "Increment Counter" };
        prefCounterBtn.Clicked += OnIncrementCounter;
        layout.Children.Add(prefCounterBtn);

        var prefClearBtn = new Button { Text = "Clear Preferences" };
        prefClearBtn.Clicked += OnClearPrefs;
        layout.Children.Add(prefClearBtn);

        // Secure Storage section
        layout.Children.Add(CreateHeader("Secure Storage"));

        _secureValueLabel = new Label
        {
            FontSize = 16, TextColor = Colors.White,
            Text = "Stored: (loading...)"
        };
        layout.Children.Add(_secureValueLabel);
        _ = LoadSecureValue();

        _secureEntry = new Entry { Placeholder = "Enter a secret", TextColor = Colors.White };
        layout.Children.Add(_secureEntry);

        var secureSaveBtn = new Button { Text = "Save Secret" };
        secureSaveBtn.Clicked += OnSaveSecret;
        layout.Children.Add(secureSaveBtn);

        var secureRemoveBtn = new Button { Text = "Remove Secret" };
        secureRemoveBtn.Clicked += OnRemoveSecret;
        layout.Children.Add(secureRemoveBtn);

        var secureRemoveAllBtn = new Button { Text = "Remove All Secrets" };
        secureRemoveAllBtn.Clicked += OnRemoveAllSecrets;
        layout.Children.Add(secureRemoveAllBtn);

        // Load counter
        _counter = Preferences.Get("test_counter", 0);

        Content = new ScrollView { Content = layout };
    }

    void OnSavePref(object? sender, EventArgs e)
    {
        var val = _prefEntry?.Text;
        if (string.IsNullOrEmpty(val))
        {
            _statusLabel.Text = "Enter a value first";
            return;
        }
        Preferences.Set(PrefKey, val);
        _prefValueLabel.Text = $"Stored: {val}";
        _statusLabel.Text = $"Preference saved: {PrefKey} = {val}";
    }

    void OnIncrementCounter(object? sender, EventArgs e)
    {
        _counter++;
        Preferences.Set("test_counter", _counter);
        _statusLabel.Text = $"Counter: {_counter} (persisted)";
    }

    void OnClearPrefs(object? sender, EventArgs e)
    {
        Preferences.Clear();
        _counter = 0;
        _prefValueLabel.Text = "Stored: (not set)";
        _statusLabel.Text = "All preferences cleared";
    }

    async void OnSaveSecret(object? sender, EventArgs e)
    {
        var val = _secureEntry?.Text;
        if (string.IsNullOrEmpty(val))
        {
            _statusLabel.Text = "Enter a secret first";
            return;
        }
        try
        {
            await SecureStorage.SetAsync(SecureKey, val);
            _secureValueLabel.Text = $"Stored: {val}";
            _statusLabel.Text = $"Secret saved to Keychain";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
    }

    async void OnRemoveSecret(object? sender, EventArgs e)
    {
        var removed = SecureStorage.Remove(SecureKey);
        _secureValueLabel.Text = "Stored: (not set)";
        _statusLabel.Text = removed ? "Secret removed" : "No secret found to remove";
    }

    async void OnRemoveAllSecrets(object? sender, EventArgs e)
    {
        SecureStorage.RemoveAll();
        _secureValueLabel.Text = "Stored: (not set)";
        _statusLabel.Text = "All secrets removed";
    }

    async Task LoadSecureValue()
    {
        try
        {
            var val = await SecureStorage.GetAsync(SecureKey);
            _secureValueLabel.Text = $"Stored: {val ?? "(not set)"}";
        }
        catch (Exception ex)
        {
            _secureValueLabel.Text = $"Error: {ex.Message}";
        }
    }

    static Label CreateHeader(string text) => new()
    {
        Text = text,
        FontSize = 22,
        FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb("#4FC3F7"),
        Margin = new Thickness(0, 15, 0, 5)
    };

    static string TryGet(Func<string> getter)
    {
        try { return getter(); }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }
}
