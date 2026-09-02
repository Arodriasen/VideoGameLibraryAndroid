using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel(App.AccountService, App.DialogService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
