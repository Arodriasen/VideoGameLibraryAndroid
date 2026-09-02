using System;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Domain.Entities;
using VideoGameLibraryAndroid.Domain.Repositories;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Envuelve un Game borrado (papelera) -- añade la cuenta atrás en días, que no existe en
    // GameCardItem porque solo tiene sentido aquí.
    public class TrashItem
    {
        public int Id { get; }
        public string Title { get; }
        public string Platform { get; }
        public ImageSource? CoverImageSource { get; }
        public bool HasCover => CoverImageSource != null;
        public string DaysLeftText { get; }

        public TrashItem(Game game)
        {
            Id = game.Id;
            Title = game.Title;
            Platform = game.Platform;
            CoverImageSource = GameCardItem.BuildCoverSource(game);

            var deletedDate = game.DeletedDate ?? DateTime.Now;
            var daysLeft = IGameRepository.TrashRetentionDays - (int)(DateTime.Now - deletedDate).TotalDays;
            DaysLeftText = daysLeft <= 0 ? "Se elimina hoy" : $"Se elimina en {daysLeft} día(s)";
        }
    }
}
