using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Core;
using VideoGameLibraryAndroid.Domain.Entities;
using VideoGameLibraryAndroid.Domain.Repositories;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Presentation.Views;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    public enum BarcodeLookupOutcome { AlreadyExists, ExistsInWishlist, NoCandidates, SingleCandidate, MultipleCandidates }

    public record BarcodeLookupResult(BarcodeLookupOutcome Outcome, List<Game> Candidates);

    public partial class GameEditViewModel : ObservableObject
    {
        private readonly IGameRepository _repository;
        private readonly IAppDialogService _dialogService;

        // El Game real ya cargado de la BD cuando se edita (no null) -- se reutiliza tal cual y
        // solo se tocan los campos que expone este formulario (Título/Plataforma/Género/
        // Editorial/Etiquetas/Año/Notas/Puntuación/Jugado/Wishlist -- mismos campos que el
        // formulario del escritorio). Si se reconstruyera un Game nuevo desde cero con solo estos
        // campos, se perderían Barcode/CoverUrl/CoverData al guardar (UpdateAsync sobrescribe la
        // fila entera) -- por eso se guarda la referencia real.
        private Domain.Entities.Game? _existingGame;

        // Portada del candidato elegido tras escanear (si hay) -- se aplica al Game real solo al
        // guardar, el formulario no tiene un control de imagen propio en esta primera versión.
        private byte[]? _pendingCoverData;
        private string? _pendingCoverUrl;

        // ObservableProperty (no una propiedad plana) a propósito: PageTitle e IsNewGame
        // dependen de GameId, y un binding de MAUI a una propiedad calculada solo se refresca si
        // se notifica el cambio -- con una propiedad plana, si GameId se fija después de que la
        // página ya evaluó el binding una vez, el título/botón de escaneo se quedarían con el
        // valor inicial (null) para siempre.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageTitle))]
        [NotifyPropertyChangedFor(nameof(IsNewGame))]
        private int? gameId;

        public bool IsNewGame => !GameId.HasValue;

        [ObservableProperty]
        private string barcode = string.Empty;

        // Solo se usa para un alta nueva (GameId == null) -- lo fija la página desde el
        // parámetro de navegación "wishlist" (viene de MainViewModel.AddAsync, según qué vista
        // estuviera abierta al pulsar +). Editar un juego existente ignora esto y usa su propio
        // IsWishlist real (ver LoadAsync).
        public bool WishlistParam { get; set; }

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string platform = string.Empty;

        [ObservableProperty]
        private string genre = string.Empty;

        [ObservableProperty]
        private string publisher = string.Empty;

        [ObservableProperty]
        private string tags = string.Empty;

        [ObservableProperty]
        private string yearText = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private int rating;

        [ObservableProperty]
        private bool played;

        [ObservableProperty]
        private bool isWishlist;

        [ObservableProperty]
        private bool isBusy;

        public string PageTitle => GameId.HasValue ? "Editar juego" : "Añadir juego";

        // Valores de Plataforma/Género ya usados en la colección, para el autocompletado de más
        // abajo -- evita duplicados tontos por mayúsculas o errores tipográficos ("Nintendo
        // Switch" vs "nintendo switch") que luego no coinciden entre sí en los filtros.
        private List<string> _knownPlatforms = new();
        private List<string> _knownGenres = new();

        public ObservableCollection<string> PlatformSuggestions { get; } = new();
        public ObservableCollection<string> GenreSuggestions { get; } = new();
        public bool HasPlatformSuggestions => PlatformSuggestions.Count > 0;
        public bool HasGenreSuggestions => GenreSuggestions.Count > 0;

        public GameEditViewModel(IGameRepository repository, IAppDialogService dialogService)
        {
            _repository = repository;
            _dialogService = dialogService;
        }

        public async Task LoadAsync()
        {
            // Se consulta siempre (alta o edición): hace falta la colección completa para las
            // sugerencias de Plataforma/Género, no solo para rellenar un juego ya existente.
            var games = await _repository.GetAllAsync();
            _knownPlatforms = games.Select(g => g.Platform)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList();
            _knownGenres = games.SelectMany(g => TextListUtils.SplitGenres(g.Genre))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(g => g).ToList();

            if (GameId == null)
            {
                IsWishlist = WishlistParam;
                return;
            }

            _existingGame = games.FirstOrDefault(g => g.Id == GameId);
            if (_existingGame == null) return;

            Title = _existingGame.Title;
            Platform = _existingGame.Platform;
            Genre = _existingGame.Genre;
            Publisher = _existingGame.Publisher;
            Tags = _existingGame.Tags;
            YearText = _existingGame.Year?.ToString() ?? string.Empty;
            Notes = _existingGame.Notes;
            Rating = _existingGame.Rating;
            Played = _existingGame.Played;
            IsWishlist = _existingGame.IsWishlist;
            Barcode = _existingGame.Barcode ?? string.Empty;

            // Al autorrellenar Platform/Genre del juego ya guardado no tiene sentido mostrar
            // sugerencias sobre el propio valor que se acaba de cargar.
            PlatformSuggestions.Clear();
            GenreSuggestions.Clear();
        }

        partial void OnPlatformChanged(string value) => UpdateSuggestions(value, _knownPlatforms, PlatformSuggestions, nameof(HasPlatformSuggestions));
        partial void OnGenreChanged(string value) => UpdateSuggestions(value, _knownGenres, GenreSuggestions, nameof(HasGenreSuggestions));

        private void UpdateSuggestions(string text, List<string> known, ObservableCollection<string> target, string hasSuggestionsPropertyName)
        {
            target.Clear();
            var trimmed = text.Trim();
            if (trimmed.Length > 0)
            {
                foreach (var candidate in known)
                {
                    // No sugerir el mismo valor que ya está escrito tal cual (mismo criterio que
                    // "sin filtros activos": no aporta nada, solo estorba).
                    if (target.Count >= 5) break;
                    if (candidate.Equals(trimmed, StringComparison.OrdinalIgnoreCase)) continue;
                    if (candidate.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
                        target.Add(candidate);
                }
            }
            OnPropertyChanged(hasSuggestionsPropertyName);
        }

        [RelayCommand]
        private void SelectPlatformSuggestion(string suggestion) => Platform = suggestion;

        [RelayCommand]
        private void SelectGenreSuggestion(string suggestion) => Genre = suggestion;

        [RelayCommand]
        private async Task ScanAsync()
        {
            await Shell.Current.GoToAsync(nameof(BarcodeScanPage));
        }

        // Llamado por GameEditPage tras volver de BarcodeScanPage con un código. Comprueba
        // primero si ya existe en la BD (evita el duplicado antes de intentarlo, más amable que
        // esperar a que el índice único de la BD lo rechace) y si no, consulta las fuentes
        // externas (App.ApiService, ya construido por App.xaml.cs). El llamador (la página)
        // decide qué mostrar según el resultado -- DisplayActionSheet vive en Page, no aquí.
        public async Task<BarcodeLookupResult> ResolveBarcodeAsync(string scannedBarcode)
        {
            Barcode = scannedBarcode;

            // Si ya está en deseados (no en la colección), no es un duplicado a rechazar -- es el
            // caso de "lo tenía en la lista de deseos y acabo de comprarlo": la página ofrece
            // pasarlo a la colección en vez de solo avisar de que ya existe.
            var existing = await _repository.GetByBarcodeAsync(scannedBarcode);
            if (existing != null)
            {
                return existing.IsWishlist
                    ? new BarcodeLookupResult(BarcodeLookupOutcome.ExistsInWishlist, new List<Game> { existing })
                    : new BarcodeLookupResult(BarcodeLookupOutcome.AlreadyExists, new List<Game>());
            }

            if (App.ApiService == null)
                return new BarcodeLookupResult(BarcodeLookupOutcome.NoCandidates, new List<Game>());

            var candidates = await App.ApiService.SearchCandidatesByBarcodeAsync(scannedBarcode);
            if (candidates.Count == 0)
                return new BarcodeLookupResult(BarcodeLookupOutcome.NoCandidates, candidates);

            if (candidates.Count == 1)
            {
                ApplyCandidate(candidates[0]);
                return new BarcodeLookupResult(BarcodeLookupOutcome.SingleCandidate, candidates);
            }

            return new BarcodeLookupResult(BarcodeLookupOutcome.MultipleCandidates, candidates);
        }

        // Reutiliza el mismo enum/registro que la resolución por código de barras -- solo cambia
        // la fuente (nombre en vez de barcode) y que no tiene sentido comprobar "ya existe": el
        // usuario está tecleando un título a mano, no escaneando algo que ya podría tener.
        public async Task<BarcodeLookupResult> ResolveByNameAsync(string name)
        {
            var trimmed = name.Trim();
            if (App.ApiService == null || trimmed.Length == 0)
                return new BarcodeLookupResult(BarcodeLookupOutcome.NoCandidates, new List<Game>());

            var candidates = await App.ApiService.SearchByNameCandidatesAsync(trimmed);
            if (candidates.Count == 0)
                return new BarcodeLookupResult(BarcodeLookupOutcome.NoCandidates, candidates);

            if (candidates.Count == 1)
            {
                ApplyCandidate(candidates[0]);
                return new BarcodeLookupResult(BarcodeLookupOutcome.SingleCandidate, candidates);
            }

            return new BarcodeLookupResult(BarcodeLookupOutcome.MultipleCandidates, candidates);
        }

        // Confirmado por el usuario en GameEditPage tras un escaneo que resolvió a
        // ExistsInWishlist: el juego ya tenía ficha (con su portada/notas/etc.), así que basta
        // con destogglear IsWishlist -- no hace falta pasar por este formulario para nada más.
        public async Task MoveToCollectionAsync(Game game)
        {
            game.IsWishlist = false;
            await _repository.UpdateAsync(game);
        }

        public void ApplyCandidate(Game candidate)
        {
            Title = candidate.Title;
            if (!string.IsNullOrEmpty(candidate.Platform)) Platform = candidate.Platform;
            if (!string.IsNullOrEmpty(candidate.Genre)) Genre = candidate.Genre;
            if (candidate.Year.HasValue) YearText = candidate.Year.Value.ToString();
            _pendingCoverData = candidate.CoverData;
            _pendingCoverUrl = candidate.CoverUrl;
        }

        // Mismo criterio que el escritorio: tocar la misma estrella ya puesta quita la puntuación
        // (vuelve a 0/sin puntuar) en vez de dejarla clavada para siempre una vez tocada.
        [RelayCommand]
        private void SetRating(string value)
        {
            var tapped = int.Parse(value);
            Rating = Rating == tapped ? 0 : tapped;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var trimmedTitle = Title.Trim();
            if (trimmedTitle.Length == 0)
            {
                await _dialogService.ShowWarningAsync("El título es obligatorio.");
                return;
            }

            IsBusy = true;
            try
            {
                var game = _existingGame ?? new Game();
                game.Title = trimmedTitle;
                game.Platform = Platform.Trim();
                game.Genre = Genre.Trim();
                game.Publisher = Publisher.Trim();
                game.Tags = Tags.Trim();
                game.Year = int.TryParse(YearText.Trim(), out var y) ? y : null;
                game.Notes = Notes.Trim();
                game.Rating = Rating;
                game.Played = Played;
                game.IsWishlist = IsWishlist;
                game.Barcode = string.IsNullOrEmpty(Barcode) ? null : Barcode;

                if (_pendingCoverData != null)
                {
                    game.CoverData = _pendingCoverData;
                    game.CoverUrl = _pendingCoverUrl ?? game.CoverUrl;
                }

                if (_existingGame != null)
                    await _repository.UpdateAsync(game);
                else
                    await _repository.AddAsync(game);

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Guardar juego", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido guardar el juego:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
