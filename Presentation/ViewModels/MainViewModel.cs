using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Domain.Entities;
using VideoGameLibraryAndroid.Domain.Repositories;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Presentation.Views;
// SplitGenres/SplitTags viven ahora en Core/TextListUtils.cs (proyecto aparte sin dependencias
// de Android, para poder testearlos con "dotnet test" sin emulador) -- using static para no
// tener que tocar cada sitio donde ya se llamaban como si fueran locales de esta clase.
using static VideoGameLibraryAndroid.Core.TextListUtils;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Mismas cuatro opciones (y mismo orden) que el ComboBox de ordenar del escritorio.
    public enum GameSortOption { TitleAsc, Platform, YearDesc, RecentlyAdded }

    public partial class MainViewModel : ObservableObject
    {
        private readonly IGameRepository _repository;
        private readonly IAppDialogService _dialogService;

        // Colección completa cargada de la BD -- SearchText/filtros/orden se aplican en memoria
        // sobre esta lista, igual que el escritorio (no hace falta ir a la base en cada tecla).
        private List<Game> _allGames = new();

        // Empieza en true para que la primera vez que aparezca MainPage sí cargue -- luego solo
        // vuelve a true cuando IGameRepository.DataChanged avisa de un cambio real (añadir/editar/
        // borrar/restaurar/importar). Antes se recargaba SIN CONDICIÓN en cada OnAppearing, lo que
        // reconstruía el CollectionView entero (Clear + Add de cada tarjeta) y perdía la posición
        // de scroll incluso si solo se había entrado a ver la ficha de un juego sin cambiar nada.
        private bool _needsReload = true;

        public ObservableCollection<GameCardItem> Games { get; } = new();

        // Mismas seis facetas de filtro que el escritorio (Plataforma/Género/Etiquetas/Año/
        // Puntuación/Estado) -- mutadas directamente por FilterViewModel (se le pasa "this" como
        // dueño al navegar a FilterPage), sin pasar por eventos ni mensajes.
        public HashSet<string> SelectedPlatforms { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedGenres { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedTags { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedYears { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<int> SelectedRatings { get; } = new();
        public HashSet<bool> SelectedPlayed { get; } = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private bool isEmpty;

        [ObservableProperty]
        private bool isWishlistView;

        [ObservableProperty]
        private bool hasActiveFilters;

        [ObservableProperty]
        private GameSortOption sortOption = GameSortOption.TitleAsc;

        public string EmptyTitle => IsWishlistView ? "Tu lista de deseos está vacía" : "Tu colección está vacía";
        public string EmptySubtitle => IsWishlistView
            ? "Pulsa + para añadir un juego que quieras conseguir"
            : "Pulsa + para añadir tu primer juego";

        // Totales por sección para los contadores del segmentado. Cuentan sobre _allGames,
        // no sobre Games, para que el número no baje al escribir en el buscador.
        public int CollectionCount => _allGames.Count(g => !g.IsWishlist);
        public int WishlistCount => _allGames.Count(g => g.IsWishlist);

        public MainViewModel(IGameRepository repository, IAppDialogService dialogService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _repository.DataChanged += () => _needsReload = true;
        }

        // Llamado desde MainPage.OnAppearing: solo va a la base de datos si de verdad hace falta.
        public Task LoadIfNeededAsync()
        {
            if (!_needsReload) return Task.CompletedTask;
            _needsReload = false;
            return LoadAsync();
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                _allGames = await _repository.GetAllAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Cargar la colección", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido cargar la colección:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private Task RefreshAsync() => LoadAsync();

        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnSortOptionChanged(GameSortOption value) => ApplyFilter();

        partial void OnIsWishlistViewChanged(bool value)
        {
            OnPropertyChanged(nameof(EmptyTitle));
            OnPropertyChanged(nameof(EmptySubtitle));
            // Cambiar de vista también limpia los filtros: las facetas de la colección y de la
            // lista de deseos no tienen por qué coincidir, y arrastrar una selección que ya no
            // aplica confundiría más que ayudaría.
            ClearAllFilters();
            ApplyFilter();
        }

        private void ClearAllFilters()
        {
            SelectedPlatforms.Clear();
            SelectedGenres.Clear();
            SelectedTags.Clear();
            SelectedYears.Clear();
            SelectedRatings.Clear();
            SelectedPlayed.Clear();
        }

        // string (no bool) a propósito: un CommandParameter de XAML es siempre string, y
        // RelayCommand&lt;bool&gt; intenta un cast directo que revienta en tiempo de ejecución
        // con InvalidCastException -- mismo problema ya documentado en el escritorio para
        // RelayCommand&lt;int&gt; con CommandParameter numérico (ver SetRating en GameEditViewModel).
        [RelayCommand]
        private void ToggleWishlistView(string wishlist) => IsWishlistView = wishlist == "true";

        private void ApplyFilter()
        {
            var text = SearchText.Trim();
            IEnumerable<Game> filtered = _allGames.Where(g => g.IsWishlist == IsWishlistView);

            if (text.Length > 0)
                filtered = filtered.Where(g =>
                    g.Title.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    g.Platform.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    (g.Barcode?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));

            if (SelectedPlatforms.Count > 0)
                filtered = filtered.Where(g => SelectedPlatforms.Contains(g.Platform));
            if (SelectedGenres.Count > 0)
                filtered = filtered.Where(g => SplitGenres(g.Genre).Any(SelectedGenres.Contains));
            if (SelectedTags.Count > 0)
                filtered = filtered.Where(g => SplitTags(g.Tags).Any(SelectedTags.Contains));
            if (SelectedYears.Count > 0)
                filtered = filtered.Where(g => g.Year.HasValue && SelectedYears.Contains(g.Year.Value.ToString()));
            if (SelectedRatings.Count > 0)
                filtered = filtered.Where(g => SelectedRatings.Contains(g.Rating));
            if (SelectedPlayed.Count > 0)
                filtered = filtered.Where(g => SelectedPlayed.Contains(g.Played));

            IEnumerable<Game> ordered = SortOption switch
            {
                GameSortOption.Platform => filtered.OrderBy(g => g.Platform).ThenBy(g => g.Title),
                GameSortOption.YearDesc => filtered.OrderByDescending(g => g.Year ?? 0).ThenBy(g => g.Title),
                GameSortOption.RecentlyAdded => filtered.OrderByDescending(g => g.AddedDate),
                _ => filtered.OrderBy(g => g.Title),
            };

            Games.Clear();
            foreach (var game in ordered)
                Games.Add(new GameCardItem(game));

            IsEmpty = Games.Count == 0;
            HasActiveFilters = SelectedPlatforms.Count > 0 || SelectedGenres.Count > 0 || SelectedTags.Count > 0 ||
                                SelectedYears.Count > 0 || SelectedRatings.Count > 0 || SelectedPlayed.Count > 0;
            OnPropertyChanged(nameof(CollectionCount));
            OnPropertyChanged(nameof(WishlistCount));
        }

        // Llamado por FilterViewModel tras aplicar/limpiar la selección de filtros.
        public void RefreshFilteredList() => ApplyFilter();

        [RelayCommand]
        private async Task OpenFiltersAsync()
        {
            var scoped = _allGames.Where(g => g.IsWishlist == IsWishlistView).ToList();

            var platforms = scoped.Select(g => g.Platform)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList();
            var genres = scoped.SelectMany(g => SplitGenres(g.Genre))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(g => g).ToList();
            var tags = scoped.SelectMany(g => SplitTags(g.Tags))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
            var years = scoped.Where(g => g.Year.HasValue).Select(g => g.Year!.Value.ToString())
                .Distinct().OrderByDescending(y => y).ToList();

            // No se pasa por Shell.GoToAsync(ruta, Dictionary&lt;string,object&gt;): con valores que
            // no son string (este MainViewModel, listas), el mecanismo interno de Shell
            // (ShellContent.ApplyQueryAttributes) intenta convertirlos con Convert.ChangeType y
            // revienta con InvalidCastException en cuanto se navega. En su lugar, campos estáticos
            // en FilterPage que se leen en su constructor -- mismo patrón que _pendingBarcode de
            // GameEditPage, adaptado a objetos en vez de a un string de la query.
            FilterPage.PendingOwner = this;
            FilterPage.PendingPlatforms = platforms;
            FilterPage.PendingGenres = genres;
            FilterPage.PendingTags = tags;
            FilterPage.PendingYears = years;

            await Shell.Current.GoToAsync(nameof(FilterPage));
        }

        [RelayCommand]
        private async Task AddAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(GameEditPage)}?wishlist={IsWishlistView}");
        }

        [RelayCommand]
        private async Task SelectGameAsync(GameCardItem? item)
        {
            if (item == null) return;
            await Shell.Current.GoToAsync($"{nameof(GameDetailPage)}?id={item.Id}");
        }
    }
}
