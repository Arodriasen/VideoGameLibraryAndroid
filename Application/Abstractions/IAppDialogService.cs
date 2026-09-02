using System.Threading.Tasks;

namespace VideoGameLibraryAndroid.Application.Abstractions
{
    public interface IAppDialogService
    {
        Task ShowInfoAsync(string message, string title = "");
        Task ShowWarningAsync(string message, string title = "");
        Task ShowErrorAsync(string message, string title = "");

        // Devuelve true solo si se pulsó "Sí".
        Task<bool> ShowConfirmAsync(string message, string title = "");
    }
}
