using System;
using System.IO;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Envuelve un Game para la tarjeta de la lista -- lógica de presentación (portada como
    // ImageSource, texto de estrellas) que no pertenece al Domain, mismo criterio que
    // CardVisibleTags/GameViewModel en el escritorio.
    public class GameCardItem
    {
        public int Id { get; }
        public string Title { get; }
        public string Platform { get; }
        public string YearText { get; }
        public int Rating { get; }

        // "5,0"/"4,0": la puntuación es siempre un entero 1-5, así que el decimal es
        // constante -- no hace falta CultureInfo para el separador de coma.
        public string RatingText => Rating > 0 ? $"{Rating},0" : string.Empty;
        public bool HasRating => Rating > 0;

        public ImageSource? CoverImageSource { get; }
        public bool HasCover => CoverImageSource != null;

        public GameCardItem(Game game)
        {
            Id = game.Id;
            Title = game.Title;
            Platform = game.Platform;
            YearText = game.Year?.ToString() ?? string.Empty;
            Rating = game.Rating;
            CoverImageSource = BuildCoverSource(game);
        }

        // internal (no private): reutilizado por TrashItem para no duplicar esta lógica.
        internal static ImageSource? BuildCoverSource(Game game)
        {
            if (game.CoverData is { Length: > 0 })
                return ImageSource.FromStream(() => new MemoryStream(game.CoverData));

            if (!string.IsNullOrEmpty(game.CoverUrl) && Uri.TryCreate(game.CoverUrl, UriKind.Absolute, out var uri))
                return ImageSource.FromUri(uri);

            return null;
        }
    }
}
