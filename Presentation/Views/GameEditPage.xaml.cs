using System.Threading.Tasks;
using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

[QueryProperty(nameof(IdParam), "id")]
[QueryProperty(nameof(WishlistParam), "wishlist")]
[QueryProperty(nameof(BarcodeParam), "barcode")]
public partial class GameEditPage : ContentPage
{
    private readonly GameEditViewModel _viewModel;
    private string? _pendingBarcode;

    public string? IdParam
    {
        set
        {
            if (int.TryParse(value, out var id))
                _viewModel.GameId = id;
        }
    }

    public string? WishlistParam
    {
        set => _viewModel.WishlistParam = value == "true";
    }

    // Llega al volver de BarcodeScanPage -- se procesa una vez en OnAppearing, no aquí, porque
    // hace falta await (consultar la BD y las APIs externas) y el setter no puede ser async.
    public string? BarcodeParam
    {
        set => _pendingBarcode = value;
    }

    public GameEditPage()
    {
        InitializeComponent();
        _viewModel = new GameEditViewModel(App.Repository!, App.DialogService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();

        if (!string.IsNullOrEmpty(_pendingBarcode))
        {
            var barcode = _pendingBarcode;
            _pendingBarcode = null; // no reprocesar si la página reaparece por otro motivo
            await HandleScannedBarcodeAsync(barcode!);
        }
    }

    private async Task HandleScannedBarcodeAsync(string barcode)
    {
        var result = await _viewModel.ResolveBarcodeAsync(barcode);

        if (result.Outcome == BarcodeLookupOutcome.AlreadyExists)
        {
            await App.DialogService.ShowWarningAsync(
                "Este código de barras ya está en tu colección.", "Ya lo tienes");
            return;
        }

        if (result.Outcome == BarcodeLookupOutcome.ExistsInWishlist)
        {
            var existing = result.Candidates[0];
            var move = await App.DialogService.ShowConfirmAsync(
                $"\"{existing.Title}\" está en tu lista de deseos. ¿Lo pasamos a tu colección?",
                "Ya lo tenías en deseados");
            if (move)
            {
                await _viewModel.MoveToCollectionAsync(existing);
                await Shell.Current.GoToAsync("..");
            }
            return;
        }

        await HandleCandidatesAsync(result, "No se ha encontrado información para este código. Rellena los datos a mano.");
    }

    // Icono de lupa junto al título -- reintenta por nombre cuando el escaneo no encontró nada
    // (o para un juego que se está dando de alta sin escanear). Mismo criterio que el escritorio
    // ("Buscar por nombre", junto al campo Título).
    private async void OnSearchByNameClicked(object? sender, System.EventArgs e)
    {
        var result = await _viewModel.ResolveByNameAsync(_viewModel.Title);
        await HandleCandidatesAsync(result, "No se ha encontrado información para ese título.");
    }

    // Compartido por escaneo y búsqueda por nombre: ambos resuelven a NoCandidates/
    // SingleCandidate/MultipleCandidates de la misma forma, solo cambia el mensaje de "sin
    // resultados" y quién ha llamado. Varios candidatos abre GameCandidatePickerPage (portada +
    // título + plataforma/año) en vez de un DisplayActionSheet de solo texto -- mismo criterio
    // que GameCandidatePickerDialog del escritorio, útil sobre todo en la búsqueda por nombre,
    // que puede devolver bastantes más resultados que el escaneo por código de barras.
    private async Task HandleCandidatesAsync(BarcodeLookupResult result, string noResultsMessage)
    {
        switch (result.Outcome)
        {
            case BarcodeLookupOutcome.NoCandidates:
                await App.DialogService.ShowInfoAsync(noResultsMessage, "Sin resultados");
                break;

            case BarcodeLookupOutcome.MultipleCandidates:
                var picked = await GameCandidatePickerPage.PickAsync(result.Candidates);
                if (picked != null)
                    _viewModel.ApplyCandidate(picked);
                break;

            case BarcodeLookupOutcome.SingleCandidate:
                // Ya se ha autorrellenado el formulario dentro de ResolveBarcodeAsync/ResolveByNameAsync.
                break;
        }
    }
}
