using VideoGameLibraryAndroid.Presentation.Views;

namespace VideoGameLibraryAndroid;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // SettingsPage, LoginPage y ForgotPasswordPage no aparecen como ShellContent (no deben
        // salir en el flyout, son pantallas de detalle) — se navega a ellas por ruta registrada
        // aquí.
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(GameDetailPage), typeof(GameDetailPage));
        Routing.RegisterRoute(nameof(GameEditPage), typeof(GameEditPage));
        Routing.RegisterRoute(nameof(BarcodeScanPage), typeof(BarcodeScanPage));
        Routing.RegisterRoute(nameof(TrashPage), typeof(TrashPage));
        Routing.RegisterRoute(nameof(FilterPage), typeof(FilterPage));
        Routing.RegisterRoute(nameof(GameCandidatePickerPage), typeof(GameCandidatePickerPage));
    }
}
