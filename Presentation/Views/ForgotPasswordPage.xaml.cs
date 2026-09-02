using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

[QueryProperty(nameof(EmailParam), "email")]
public partial class ForgotPasswordPage : ContentPage
{
    private readonly ForgotPasswordViewModel _viewModel;

    public string? EmailParam
    {
        set => _viewModel.Email = value ?? string.Empty;
    }

    public ForgotPasswordPage()
    {
        InitializeComponent();
        _viewModel = new ForgotPasswordViewModel(App.AccountService, App.DialogService);
        BindingContext = _viewModel;
    }
}
