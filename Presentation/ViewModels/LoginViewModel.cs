using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Application.Models;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Presentation.Views;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly IAppDialogService _dialogService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe = true;

        [ObservableProperty]
        private bool isBusy;

        public LoginViewModel(IAccountService accountService, IAppDialogService dialogService)
        {
            _accountService = accountService;
            _dialogService = dialogService;
        }

        public async Task LoadAsync()
        {
            var remembered = await App.LoadRememberedLoginAsync();
            if (remembered != null)
            {
                Email = remembered.Email;
                Password = remembered.Password;
            }
        }

        [RelayCommand]
        private async Task SignInAsync()
        {
            if (!await ValidateInputAsync()) return;

            IsBusy = true;
            try
            {
                await _accountService.SignInAsync(Email.Trim(), Password);
                await ResolveAfterLoginAsync();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Iniciar sesión", ex);
                await _dialogService.ShowErrorAsync(
                    $"No se ha podido iniciar sesión. Revisa tu correo y contraseña:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            if (!await ValidateInputAsync()) return;

            IsBusy = true;
            try
            {
                var result = await _accountService.SignUpAsync(Email.Trim(), Password);
                if (result == SignUpResult.ConfirmationRequired)
                {
                    await _dialogService.ShowInfoAsync(
                        "Cuenta creada. Revisa tu correo para confirmarla y luego inicia sesión aquí.",
                        "Confirma tu correo");
                    return;
                }

                await ResolveAfterLoginAsync();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Crear cuenta", ex);
                await _dialogService.ShowErrorAsync($"No se ha podido crear la cuenta:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToForgotPasswordAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(ForgotPasswordPage)}?email={Uri.EscapeDataString(Email.Trim())}");
        }

        private async Task<bool> ValidateInputAsync()
        {
            if (Email.Trim().Length > 0 && Password.Length > 0) return true;

            await _dialogService.ShowWarningAsync("Introduce tu correo y contraseña.");
            return false;
        }

        // Tras iniciar sesión o crear cuenta con éxito: guarda o borra las credenciales
        // recordadas según la casilla, y si la cuenta está vacía (nueva) pero este dispositivo ya
        // tenía una configuración local, ofrece subirla una vez. Termina volviendo a MainPage,
        // que retoma el arranque normal (conectar con la cadena que haya, propia o recién subida).
        private async Task ResolveAfterLoginAsync()
        {
            if (RememberMe)
                await App.SaveRememberedLoginAsync(Email.Trim(), Password);
            else
                App.ClearRememberedLogin();

            var remote = await _accountService.GetSettingsAsync();
            if (remote == null || string.IsNullOrEmpty(remote.ConnectionString))
            {
                var local = await App.LoadConfigAsync();
                if (!string.IsNullOrEmpty(local.ConnectionString))
                {
                    var upload = await _dialogService.ShowConfirmAsync(
                        "Este dispositivo ya tiene una configuración guardada (cadena de conexión y claves de API). " +
                        "¿Quieres subirla a tu cuenta para tenerla disponible en tus demás dispositivos?",
                        "Sincronizar configuración");

                    if (upload)
                    {
                        try
                        {
                            await _accountService.SaveSettingsAsync(new UserSettings
                            {
                                ConnectionString = local.ConnectionString,
                                ScanDexToken = local.ScanDexToken,
                                IgdbClientId = local.IgdbClientId,
                                IgdbClientSecret = local.IgdbClientSecret,
                                RawgApiKey = local.RawgApiKey,
                                TheGamesDbApiKey = local.TheGamesDbApiKey
                            });
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogError("Subir configuración local a la cuenta", ex);
                            await _dialogService.ShowErrorAsync($"No se ha podido subir la configuración a tu cuenta:\n{ex.Message}");
                        }
                    }
                }
            }

            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
    }
}
