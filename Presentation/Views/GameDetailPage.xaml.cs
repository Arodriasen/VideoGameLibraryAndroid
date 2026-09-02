using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

[QueryProperty(nameof(IdParam), "id")]
public partial class GameDetailPage : ContentPage
{
    private readonly GameDetailViewModel _viewModel;

    public string? IdParam
    {
        set
        {
            if (int.TryParse(value, out var id))
                _viewModel.GameId = id;
        }
    }

    public GameDetailPage()
    {
        InitializeComponent();
        _viewModel = new GameDetailViewModel(App.Repository!, App.DialogService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
