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
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IAppDialogService _dialogService;

        // Primer arranque = no hay ninguna cadena de conexión guardada todavía. Lo fija
        // SettingsPage (vía IQueryAttributable) a partir del parámetro de navegación "firstRun".
        // Cambia a dónde navega el botón Guardar al terminar (ver SaveAsync) y oculta el botón
        // de cancelar (no tiene sentido cancelar si todavía no hay ninguna base conectada).
        public bool FirstRun { get; set; }

        [ObservableProperty]
        private string connectionString = string.Empty;

        [ObservableProperty]
        private string scanDexToken = string.Empty;

        [ObservableProperty]
        private string igdbClientId = string.Empty;

        [ObservableProperty]
        private string igdbClientSecret = string.Empty;

        [ObservableProperty]
        private string rawgApiKey = string.Empty;

        [ObservableProperty]
        private string theGamesDbApiKey = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        public SettingsViewModel(IAppDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task LoadAsync()
        {
            var config = await App.LoadConfigAsync();
            ConnectionString = config.ConnectionString;
            ScanDexToken = config.ScanDexToken;
            IgdbClientId = config.IgdbClientId;
            IgdbClientSecret = config.IgdbClientSecret;
            RawgApiKey = config.RawgApiKey;
            TheGamesDbApiKey = config.TheGamesDbApiKey;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var trimmedConnectionString = ConnectionString.Trim();
            if (trimmedConnectionString.Length == 0)
            {
                await _dialogService.ShowWarningAsync(
                    "Introduce la cadena de conexión de tu base de datos en Neon antes de continuar.",
                    "Conexión a la base de datos");
                return;
            }

            IsBusy = true;
            try
            {
                await App.ReconnectAsync(trimmedConnectionString);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Conectar con la base de datos Neon", ex);
                await _dialogService.ShowErrorAsync(
                    $"No se ha podido conectar con la base de datos. Revisa la cadena de conexión y tu conexión a internet:\n{ex.Message}",
                    "Conexión a la base de datos");
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await App.SaveApiKeysAsync(
                ScanDexToken.Trim(), IgdbClientId.Trim(), IgdbClientSecret.Trim(),
                RawgApiKey.Trim(), TheGamesDbApiKey.Trim());

            await SyncToAccountAsync();

            if (FirstRun)
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            else
                await Shell.Current.GoToAsync("..");
        }

        // Empuja los ajustes actuales a la cuenta (Supabase) para que estén disponibles en los
        // demás dispositivos la próxima vez que inicien sesión. Best-effort: un fallo aquí (sin
        // sesión, sin red, etc.) no debe impedir haber guardado ya localmente justo antes.
        private async Task SyncToAccountAsync()
        {
            try
            {
                await App.AccountService.SaveSettingsAsync(new UserSettings
                {
                    ConnectionString = ConnectionString.Trim(),
                    ScanDexToken = ScanDexToken.Trim(),
                    IgdbClientId = IgdbClientId.Trim(),
                    IgdbClientSecret = IgdbClientSecret.Trim(),
                    RawgApiKey = RawgApiKey.Trim(),
                    TheGamesDbApiKey = TheGamesDbApiKey.Trim()
                });
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Sincronizar ajustes con la cuenta", ex);
            }
        }
    }
}
