using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Domain.Repositories;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Presentation.Views;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    public partial class GameDetailViewModel : ObservableObject
    {
        private readonly IGameRepository _repository;
        private readonly IAppDialogService _dialogService;

        // Referencia real del Game cargado -- igual que _existingGame en GameEditViewModel: los
        // toggles de Jugado/Deseados solo tocan su campo correspondiente y reutilizan el resto tal
        // cual al guardar (UpdateAsync sobrescribe la fila entera).
        private VideoGameLibraryAndroid.Domain.Entities.Game? _game;

        public int GameId { get; set; }

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string platform = string.Empty;

        [ObservableProperty]
        private string genre = string.Empty;

        [ObservableProperty]
        private string publisher = string.Empty;

        [ObservableProperty]
        private string yearText = string.Empty;

        [ObservableProperty]
        private string barcode = string.Empty;

        [ObservableProperty]
        private bool hasBarcode;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private bool hasNotes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RatingText))]
        [NotifyPropertyChangedFor(nameof(HasRating))]
        private int rating;

        // "5,0": la puntuación es siempre un entero 1-5, no hace falta CultureInfo para la coma.
        public string RatingText => Rating > 0 ? $"{Rating},0" : string.Empty;
        public bool HasRating => Rating > 0;

        [ObservableProperty]
        private bool played;

        [ObservableProperty]
        private string addedDateText = string.Empty;

        [ObservableProperty]
        private ImageSource? coverImageSource;

        [ObservableProperty]
        private bool hasCover;

        [ObservableProperty]
        private bool isWishlist;

        // Mismo criterio que GameViewModel.TagList del escritorio: Tags se guarda como un único
        // texto separado por comas, se separa aquí para mostrar cada etiqueta como un chip suelto.
        public List<string> TagList { get; private set; } = new();

        public bool HasTags => TagList.Count > 0;

        public GameDetailViewModel(IGameRepository repository, IAppDialogService dialogService)
        {
            _repository = repository;
            _dialogService = dialogService;
        }

        public async Task LoadAsync()
        {
            var games = await _repository.GetAllAsync();
            var game = games.FirstOrDefault(g => g.Id == GameId);
            if (game == null) return;

            _game = game;

            Title = game.Title;
            Platform = game.Platform;
            Genre = game.Genre;
            Publisher = game.Publisher;
            YearText = game.Year?.ToString() ?? string.Empty;
            Barcode = game.Barcode ?? string.Empty;
            HasBarcode = !string.IsNullOrEmpty(Barcode);
            Notes = game.Notes;
            HasNotes = !string.IsNullOrWhiteSpace(Notes);
            Rating = game.Rating;
            Played = game.Played;
            IsWishlist = game.IsWishlist;
            AddedDateText = FormatAddedDate(game.AddedDate);
            TagList = MainViewModel.SplitTags(game.Tags).ToList();
            OnPropertyChanged(nameof(TagList));
            OnPropertyChanged(nameof(HasTags));

            var card = new GameCardItem(game);
            CoverImageSource = card.CoverImageSource;
            HasCover = card.HasCover;
        }

        // "14 mar 2024": array fijo en vez de CultureInfo("es-ES") -- mismo criterio que el resto
        // de la app, que no depende de datos de localización del sistema para textos en español.
        private static readonly string[] MonthAbbreviations =
            { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

        private static string FormatAddedDate(DateTime date) =>
            $"{date.Day} {MonthAbbreviations[date.Month - 1]} {date.Year}";

        [RelayCommand]
        private async Task TogglePlayedAsync()
        {
            if (_game == null) return;

            var newValue = !Played;
            try
            {
                _game.Played = newValue;
                await _repository.UpdateAsync(_game);
                Played = newValue;
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Marcar como jugado", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido guardar el cambio:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ToggleWishlistAsync()
        {
            if (_game == null) return;

            var newValue = !IsWishlist;
            try
            {
                _game.IsWishlist = newValue;
                await _repository.UpdateAsync(_game);
                IsWishlist = newValue;
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Mover a lista de deseos", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido guardar el cambio:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(GameEditPage)}?id={GameId}");
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            var confirm = await _dialogService.ShowConfirmAsync(
                "Se moverá a la papelera. Podrás recuperarlo durante 7 días.", "Eliminar juego");
            if (!confirm) return;

            try
            {
                await _repository.DeleteAsync(GameId);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Eliminar juego", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido eliminar el juego:\n{ex.Message}");
            }
        }
    }
}
