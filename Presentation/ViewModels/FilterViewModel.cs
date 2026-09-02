using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Popup de filtros de MainPage, como página completa (MAUI no trae un equivalente al
    // PopupBox de MaterialDesignThemes que usa el escritorio). Mismas seis facetas que el
    // escritorio (Plataforma/Género/Etiquetas/Año/Puntuación/Estado). Recibe el MainViewModel
    // dueño y las cuatro listas derivadas de datos (Plataforma/Género/Etiquetas/Año) por
    // navegación (campos estáticos de FilterPage, no query string -- así se pasan listas reales
    // sin serializarlas a texto) y escribe el resultado directamente en las seis colecciones
    // Selected* de owner al aplicar. Puntuación y Estado no dependen de los datos (mismo criterio
    // que RebuildFixedFacet del escritorio): sus etiquetas están fijas aquí mismo.
    public partial class FilterViewModel : ObservableObject
    {
        // Mismas etiquetas y mismo orden que RatingLabels/PlayedLabels del escritorio.
        private static readonly string[] RatingLabels = { "★★★★★", "★★★★", "★★★", "★★", "★", "Sin puntuar" };
        private static readonly Dictionary<string, int> RatingLabelToValue = new()
        {
            ["★★★★★"] = 5,
            ["★★★★"] = 4,
            ["★★★"] = 3,
            ["★★"] = 2,
            ["★"] = 1,
            ["Sin puntuar"] = 0,
        };

        private static readonly string[] PlayedLabels = { "Jugados", "Falta por jugar" };
        private static readonly Dictionary<string, bool> PlayedLabelToValue = new()
        {
            ["Jugados"] = true,
            ["Falta por jugar"] = false,
        };

        private MainViewModel? _owner;
        private List<string>? _platforms;
        private List<string>? _genres;
        private List<string>? _tags;
        private List<string>? _years;

        public ObservableCollection<FilterOptionItem> Platforms { get; } = new();
        public ObservableCollection<FilterOptionItem> Genres { get; } = new();
        public ObservableCollection<FilterOptionItem> Tags { get; } = new();
        public ObservableCollection<FilterOptionItem> Years { get; } = new();
        public ObservableCollection<FilterOptionItem> Ratings { get; } = new();
        public ObservableCollection<FilterOptionItem> Played { get; } = new();

        public bool HasPlatforms => Platforms.Count > 0;
        public bool HasGenres => Genres.Count > 0;
        public bool HasTags => Tags.Count > 0;
        public bool HasYears => Years.Count > 0;

        // Los cinco setters de FilterPage (QueryProperty-like, ver comentario ahí) pueden llegar
        // en cualquier orden -- solo se guardan aquí, la construcción real de las listas se hace
        // en Initialize(), llamada desde FilterPage.OnAppearing una vez que ya han llegado todos.
        public void SetOwner(MainViewModel? owner) => _owner = owner;
        public void SetPlatforms(List<string>? platforms) => _platforms = platforms;
        public void SetGenres(List<string>? genres) => _genres = genres;
        public void SetTags(List<string>? tags) => _tags = tags;
        public void SetYears(List<string>? years) => _years = years;

        public void Initialize()
        {
            Platforms.Clear();
            foreach (var p in _platforms ?? Enumerable.Empty<string>())
                Platforms.Add(new FilterOptionItem(p, _owner?.SelectedPlatforms.Contains(p) ?? false));

            Genres.Clear();
            foreach (var g in _genres ?? Enumerable.Empty<string>())
                Genres.Add(new FilterOptionItem(g, _owner?.SelectedGenres.Contains(g) ?? false));

            Tags.Clear();
            foreach (var t in _tags ?? Enumerable.Empty<string>())
                Tags.Add(new FilterOptionItem(t, _owner?.SelectedTags.Contains(t) ?? false));

            Years.Clear();
            foreach (var y in _years ?? Enumerable.Empty<string>())
                Years.Add(new FilterOptionItem(y, _owner?.SelectedYears.Contains(y) ?? false));

            Ratings.Clear();
            foreach (var label in RatingLabels)
                Ratings.Add(new FilterOptionItem(label, _owner?.SelectedRatings.Contains(RatingLabelToValue[label]) ?? false));

            Played.Clear();
            foreach (var label in PlayedLabels)
                Played.Add(new FilterOptionItem(label, _owner?.SelectedPlayed.Contains(PlayedLabelToValue[label]) ?? false));

            OnPropertyChanged(nameof(HasPlatforms));
            OnPropertyChanged(nameof(HasGenres));
            OnPropertyChanged(nameof(HasTags));
            OnPropertyChanged(nameof(HasYears));
        }

        [RelayCommand]
        private async Task ApplyAsync()
        {
            if (_owner != null)
            {
                _owner.SelectedPlatforms.Clear();
                foreach (var p in Platforms.Where(x => x.IsSelected)) _owner.SelectedPlatforms.Add(p.Name);

                _owner.SelectedGenres.Clear();
                foreach (var g in Genres.Where(x => x.IsSelected)) _owner.SelectedGenres.Add(g.Name);

                _owner.SelectedTags.Clear();
                foreach (var t in Tags.Where(x => x.IsSelected)) _owner.SelectedTags.Add(t.Name);

                _owner.SelectedYears.Clear();
                foreach (var y in Years.Where(x => x.IsSelected)) _owner.SelectedYears.Add(y.Name);

                _owner.SelectedRatings.Clear();
                foreach (var r in Ratings.Where(x => x.IsSelected)) _owner.SelectedRatings.Add(RatingLabelToValue[r.Name]);

                _owner.SelectedPlayed.Clear();
                foreach (var p in Played.Where(x => x.IsSelected)) _owner.SelectedPlayed.Add(PlayedLabelToValue[p.Name]);

                _owner.RefreshFilteredList();
            }

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void ClearAll()
        {
            foreach (var p in Platforms) p.IsSelected = false;
            foreach (var g in Genres) g.IsSelected = false;
            foreach (var t in Tags) t.IsSelected = false;
            foreach (var y in Years) y.IsSelected = false;
            foreach (var r in Ratings) r.IsSelected = false;
            foreach (var p in Played) p.IsSelected = false;
        }
    }
}
