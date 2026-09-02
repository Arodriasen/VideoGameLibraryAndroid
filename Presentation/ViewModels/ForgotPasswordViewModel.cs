using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Infrastructure.Logging;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Recuperación por código (no por enlace web): el enlace de "restablecer contraseña" que
    // envía Supabase por defecto está pensado para que lo abra un navegador y le redirija de
    // vuelta a una web propia -- una app móvil no puede capturar esa redirección sin registrar
    // un esquema de enlaces propio. Se usa en su lugar el código de 6 dígitos que Supabase
    // también manda si la plantilla de email incluye {{ .Token }} (mismo ajuste ya hecho en el
    // proyecto Supabase compartido con el escritorio).
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly IAppDialogService _dialogService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string code = string.Empty;

        [ObservableProperty]
        private string newPassword = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isCodeStep;

        public ForgotPasswordViewModel(IAccountService accountService, IAppDialogService dialogService)
        {
            _accountService = accountService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task SendCodeAsync()
        {
            if (Email.Trim().Length == 0)
            {
                await _dialogService.ShowWarningAsync("Introduce tu correo electrónico.");
                return;
            }

            IsBusy = true;
            try
            {
                await _accountService.RequestPasswordResetAsync(Email.Trim());
                IsCodeStep = true;
                await _dialogService.ShowInfoAsync(
                    "Si la cuenta existe, te hemos enviado un código por correo. Si no lo ves en unos minutos, revisa también la carpeta de spam.",
                    "Código enviado");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Solicitar código de recuperación", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido enviar el código:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (Code.Trim().Length == 0 || NewPassword.Length == 0)
            {
                await _dialogService.ShowWarningAsync("Introduce el código y la contraseña nueva.");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await _dialogService.ShowWarningAsync("Las dos contraseñas no coinciden.");
                return;
            }

            IsBusy = true;
            try
            {
                await _accountService.ResetPasswordAsync(Email.Trim(), Code.Trim(), NewPassword);
                await _dialogService.ShowInfoAsync("Contraseña cambiada. Ya puedes iniciar sesión con ella.", "Contraseña actualizada");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Restablecer contraseña", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido cambiar la contraseña. Revisa el código:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
