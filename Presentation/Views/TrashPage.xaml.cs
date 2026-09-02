using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

public partial class TrashPage : ContentPage
{
    private readonly TrashViewModel _viewModel;

    public TrashPage()
    {
        InitializeComponent();
        _viewModel = new TrashViewModel(App.Repository!, App.DialogService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
