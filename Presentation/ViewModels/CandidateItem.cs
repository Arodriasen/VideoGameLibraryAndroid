using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Envuelve un candidato devuelto por la API (aún sin guardar, Id=0) para
    // GameCandidatePickerPage -- igual que GameCardItem pero conservando la referencia al Game
    // real, que es lo que hay que devolver al elegir uno.
    public class CandidateItem
    {
        public Game Game { get; }
        public string Title => Game.Title;
        public string Platform => Game.Platform;
        public string YearText => Game.Year?.ToString() ?? string.Empty;

        public ImageSource? CoverImageSource { get; }
        public bool HasCover => CoverImageSource != null;

        public CandidateItem(Game game)
        {
            Game = game;
            CoverImageSource = GameCardItem.BuildCoverSource(game);
        }
    }
}
