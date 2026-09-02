using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGameLibraryAndroid.Domain.Entities;
using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

// Selector de candidatos cuando una búsqueda (por código de barras o por nombre) devuelve varios
// resultados -- equivalente a GameCandidatePickerDialog del escritorio (portada + título +
// plataforma/año, en vez del DisplayActionSheet de solo texto que se usaba antes aquí).
// No se registra como ShellContent: se navega a él solo desde PickAsync.
public partial class GameCandidatePickerPage : ContentPage
{
    // Mismo patrón que FilterPage.PendingOwner/... -- Shell.GoToAsync(ruta, Dictionary<string,
    // object>) revienta con InvalidCastException si algún valor no es string, así que los datos
    // no-string se pasan por campos estáticos fijados justo antes de navegar.
    private static List<Game>? _pendingCandidates;
    private static TaskCompletionSource<Game?>? _pendingResult;

    private readonly TaskCompletionSource<Game?> _tcs;

    public GameCandidatePickerPage()
    {
        InitializeComponent();

        var candidates = _pendingCandidates ?? new List<Game>();
        _tcs = _pendingResult ?? new TaskCompletionSource<Game?>();
        _pendingCandidates = null;
        _pendingResult = null;

        // Del más nuevo al más antiguo (los que no tienen año conocido, al final) -- pedido
        // explícito del usuario tras ver una búsqueda por nombre con muchos resultados (p.ej.
        // "Zelda") sin ningún orden reconocible, el que devuelve cada API tal cual.
        CandidatesList.ItemsSource = candidates
            .OrderByDescending(g => g.Year ?? int.MinValue)
            .Select(g => new CandidateItem(g))
            .ToList();
    }

    public static async Task<Game?> PickAsync(List<Game> candidates)
    {
        var tcs = new TaskCompletionSource<Game?>();
        _pendingCandidates = candidates;
        _pendingResult = tcs;

        await Shell.Current.GoToAsync(nameof(GameCandidatePickerPage));
        return await tcs.Task;
    }

    // El TapGestureRecognizer hereda el BindingContext del Border al que está enganchado (el
    // CandidateItem de esa fila) -- no hace falta un ViewModel con RelayCommand solo para esto.
    private async void OnCandidateTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TapGestureRecognizer { BindingContext: CandidateItem item })
        {
            _tcs.TrySetResult(item.Game);
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnCancelClicked(object? sender, System.EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Shell.Current.GoToAsync("..");
    }

    // Red de seguridad: si la página se cierra de cualquier otra forma (botón físico Atrás de
    // Android), TrySetResult no hace nada si ya se resolvió por selección o por Cancelar -- pero
    // sin esto, PickAsync se quedaría esperando para siempre.
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tcs.TrySetResult(null);
    }
}
