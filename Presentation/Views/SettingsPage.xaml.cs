using System.Collections.Generic;
using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

[QueryProperty(nameof(FirstRunParam), "firstRun")]
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    // MAUI Shell pasa los parámetros de consulta como string; se traduce a bool aquí y se
    // reenvía al ViewModel (que no depende de tipos de Shell/navegación directamente).
    public string? FirstRunParam
    {
        set => _viewModel.FirstRun = value == "true";
    }

    public SettingsPage()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel(App.DialogService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
