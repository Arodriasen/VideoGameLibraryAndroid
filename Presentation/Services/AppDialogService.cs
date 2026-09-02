using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Application.Abstractions;

namespace VideoGameLibraryAndroid.Presentation.Services
{
    public class AppDialogService : IAppDialogService
    {
        public Task ShowInfoAsync(string message, string title = "")
            => Shell.Current.DisplayAlert(string.IsNullOrEmpty(title) ? "Aviso" : title, message, "OK");

        public Task ShowWarningAsync(string message, string title = "")
            => Shell.Current.DisplayAlert(string.IsNullOrEmpty(title) ? "Atención" : title, message, "OK");

        public Task ShowErrorAsync(string message, string title = "")
            => Shell.Current.DisplayAlert(string.IsNullOrEmpty(title) ? "Error" : title, message, "OK");

        public Task<bool> ShowConfirmAsync(string message, string title = "")
            => Shell.Current.DisplayAlert(string.IsNullOrEmpty(title) ? "Confirmar" : title, message, "Sí", "No");
    }
}
