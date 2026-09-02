using System.Collections.Generic;
using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

public partial class FilterPage : ContentPage
{
    // Fijados por MainViewModel.OpenFiltersAsync justo antes de navegar aquí (ver comentario ahí
    // sobre por qué no se pasan por Shell.GoToAsync con un Dictionary). Se leen y se limpian en
    // el constructor, que corre de forma síncrona durante ese mismo GoToAsync.
    public static MainViewModel? PendingOwner;
    public static List<string>? PendingPlatforms;
    public static List<string>? PendingGenres;
    public static List<string>? PendingTags;
    public static List<string>? PendingYears;

    private readonly FilterViewModel _viewModel;

    public FilterPage()
    {
        InitializeComponent();

        _viewModel = new FilterViewModel();
        _viewModel.SetOwner(PendingOwner);
        _viewModel.SetPlatforms(PendingPlatforms);
        _viewModel.SetGenres(PendingGenres);
        _viewModel.SetTags(PendingTags);
        _viewModel.SetYears(PendingYears);
        PendingOwner = null;
        PendingPlatforms = null;
        PendingGenres = null;
        PendingTags = null;
        PendingYears = null;

        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialize();
    }
}
