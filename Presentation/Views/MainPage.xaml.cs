using System.Threading.Tasks;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Presentation.ViewModels;

namespace VideoGameLibraryAndroid.Presentation.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel? _viewModel;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (App.Repository == null)
        {
            await ConnectAsync();
            if (App.Repository == null) return; // ya navegó a Login/Ajustes, o falló
        }

        ConnectingLayer.IsVisible = false;
        ContentLayer.IsVisible = true;

        _viewModel ??= new MainViewModel(App.Repository!, App.DialogService);
        BindingContext = _viewModel;
        await _viewModel.LoadIfNeededAsync();
    }

    // Primer arranque de la sesión de la app: comprobar login y conectar con la base de datos.
    // Una vez App.Repository queda asignado en una ejecución, esto no se repite en apariciones
    // posteriores de la página (ver comprobación en OnAppearing).
    private async System.Threading.Tasks.Task ConnectAsync()
    {
        LoadingIndicator.IsRunning = true;
        LblStatus.Text = string.Empty;

        try
        {
            var loggedIn = await App.AccountService.TryRestoreSessionAsync();
            if (!loggedIn)
            {
                await Shell.Current.GoToAsync(nameof(LoginPage));
                return;
            }

            var config = await App.LoadConfigAsync();

            try
            {
                var remote = await App.AccountService.GetSettingsAsync();
                if (remote != null && !string.IsNullOrEmpty(remote.ConnectionString))
                {
                    await App.ReconnectAsync(remote.ConnectionString);
                    await App.SaveApiKeysAsync(remote.ScanDexToken, remote.IgdbClientId,
                        remote.IgdbClientSecret, remote.RawgApiKey, remote.TheGamesDbApiKey);
                }
            }
            catch (System.Exception ex)
            {
                LoggingService.LogError("Recuperar ajustes de la cuenta al arrancar", ex);
            }

            if (App.Repository == null)
            {
                if (string.IsNullOrEmpty(config.ConnectionString))
                {
                    await Shell.Current.GoToAsync($"{nameof(SettingsPage)}?firstRun=true");
                    return;
                }

                await App.ReconnectAsync(config.ConnectionString);
                App.InitializeApiService(config);
            }
        }
        catch (System.Exception ex)
        {
            LoggingService.LogError("Conectar al arrancar MainPage", ex);
            LblStatus.Text = "No se ha podido conectar con la base de datos.";
            await App.DialogService.ShowErrorAsync(
                $"Revisa la cadena de conexión en Ajustes y tu conexión a internet:\n{ex.Message}",
                "Conexión a la base de datos");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnSettingsClicked(object? sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    private async void OnTrashClicked(object? sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TrashPage));
    }

    // Decompilado Microsoft.Maui.Controls.dll para confirmar la causa real del naranja: no es
    // ningún VisualState de MAUI, es SelectableViewHolder.SetSelectionStates pintando
    // Background/Foreground nativos directamente en el ItemView de Android (resuelto de
    // android:attr/colorControlHighlight), un nivel por encima de este Border -- de ahí que
    // ningún Setter puesto en la tarjeta pudiera nunca tocarlo. Con SelectionMode="None" ese
    // código de Android ni se ejecuta, así que el resaltado se hace aquí a mano: un flash breve
    // del velo morado antes de navegar. Confirmado también por decompilación que
    // PointerGestureRecognizer.PointerPressed/Released nunca se disparan en Android (solo están
    // cableados a MotionEventActions.Hover*, es decir, solo a ratón/stylus, nunca al dedo) --
    // este Tapped normal es el único evento de este Border que sí está garantizado.
    //
    // Regresión encontrada por decompilación (no era el Parent, era el sender): TapGestureHandler
    // (Platform) llama a TapGestureRecognizer.SendTapped(view, ...) pasando la View propietaria
    // como primer argumento, y SendTapped hace Tapped?.Invoke(sender, ...) reenviando ESE
    // argumento -- el "sender" de este evento es siempre el Border tocado, nunca el propio
    // TapGestureRecognizer. El "is TapGestureRecognizer" de antes no podía cumplirse jamás.
    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border { BindingContext: GameCardItem item } border) return;

        if (border.FindByName("PressOverlay") is BoxView overlay)
        {
            overlay.Opacity = 0.3;
            await Task.Delay(120);
            overlay.Opacity = 0;
        }

        if (_viewModel != null)
            await _viewModel.SelectGameCommand.ExecuteAsync(item);
    }

    // DisplayActionSheet necesita un Page -- por eso vive aquí y no en el ViewModel -- mismo
    // criterio que el ActionSheet de "¿Cuál es tu juego?" en GameEditPage.
    private async void OnSortClicked(object? sender, System.EventArgs e)
    {
        if (_viewModel == null) return;

        // Mismas cuatro opciones y mismo orden que el ComboBox "Ordenar" del escritorio.
        var choice = await DisplayActionSheet("Ordenar por", "Cancelar", null,
            "Título (A-Z)", "Plataforma", "Año (más reciente)", "Añadido recientemente");

        if (choice == null || choice == "Cancelar") return; // no tocar el orden actual

        _viewModel.SortOption = choice switch
        {
            "Plataforma" => GameSortOption.Platform,
            "Año (más reciente)" => GameSortOption.YearDesc,
            "Añadido recientemente" => GameSortOption.RecentlyAdded,
            _ => GameSortOption.TitleAsc,
        };
    }
}
