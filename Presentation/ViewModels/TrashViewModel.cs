using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Domain.Repositories;
using VideoGameLibraryAndroid.Infrastructure.Logging;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Solo borrado/restauración de uno en uno a propósito (decisión del usuario) -- sin "vaciar
    // papelera" ni selección múltiple, a diferencia del escritorio.
    public partial class TrashViewModel : ObservableObject
    {
        private readonly IGameRepository _repository;
        private readonly IAppDialogService _dialogService;

        public ObservableCollection<TrashItem> Items { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isEmpty;

        public TrashViewModel(IGameRepository repository, IAppDialogService dialogService)
        {
            _repository = repository;
            _dialogService = dialogService;
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var trash = await _repository.GetTrashAsync();
                Items.Clear();
                foreach (var game in trash)
                    Items.Add(new TrashItem(game));

                IsEmpty = Items.Count == 0;
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Cargar la papelera", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido cargar la papelera:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RestoreAsync(TrashItem? item)
        {
            if (item == null) return;

            try
            {
                await _repository.RestoreAsync(item.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Restaurar juego de la papelera", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido restaurar el juego:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteForeverAsync(TrashItem? item)
        {
            if (item == null) return;

            var confirm = await _dialogService.ShowConfirmAsync(
                $"\"{item.Title}\" se eliminará para siempre. Esta acción no se puede deshacer.", "Eliminar definitivamente");
            if (!confirm) return;

            try
            {
                await _repository.PermanentlyDeleteAsync(item.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Eliminar juego definitivamente", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido eliminar el juego:\n{ex.Message}");
            }
        }
    }
}
