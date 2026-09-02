using Microsoft.Maui.Storage;
using VideoGameLibraryAndroid.Application.Abstractions;
using VideoGameLibraryAndroid.Domain.Repositories;
using VideoGameLibraryAndroid.Infrastructure.Accounts;
using VideoGameLibraryAndroid.Infrastructure.ExternalApis;
using VideoGameLibraryAndroid.Infrastructure.Persistence;
using VideoGameLibraryAndroid.Presentation.Services;

namespace VideoGameLibraryAndroid;

// Composition root: el único sitio del proyecto que conoce las cuatro capas a la vez y las
// conecta. Domain/Application no dependen de nada de aquí; Infrastructure implementa las
// interfaces de Application; Presentation solo ve esas interfaces (vía las propiedades
// estáticas de abajo), nunca los tipos concretos de Infrastructure.
//
// Base fully-qualificada a propósito: "Application" es también un namespace hijo directo de
// VideoGameLibraryAndroid (VideoGameLibraryAndroid.Application), y el compilador lo resolvería
// a ese namespace antes que a Microsoft.Maui.Controls.Application si se escribiera sin
// cualificar aquí (mismo problema ya documentado en VideoGameLibrary de escritorio con
// System.Windows.Application).
public partial class App : Microsoft.Maui.Controls.Application
{
    public static IGameRepository? Repository { get; private set; }
    public static IGameApiService? ApiService { get; private set; }
    public static IAppDialogService DialogService { get; } = new AppDialogService();
    public static IImportService ImportService { get; } = new Infrastructure.Files.ImportService();
    public static IExportService ExportService { get; } = new Infrastructure.Files.ExportService();
    public static IUpdateCheckService UpdateCheckService { get; } = new UpdateCheckService();

    // Mismo proyecto Supabase y misma tabla user_settings que usa VideoGameLibrary (escritorio)
    // -- cuenta y ajustes se comparten a propósito entre dispositivos. La anon key es pública
    // por diseño (como una config de Firebase): Row Level Security es lo que protege los datos
    // de cada usuario, no el secreto de esta clave. Los valores en sí viven en
    // Infrastructure/Accounts/SupabaseConfig.cs, fuera del repo (ver .gitignore y el .example
    // junto a ese archivo) para no dejar la URL del proyecto visible en GitHub.
    public static IAccountService AccountService { get; } =
        new SupabaseAccountService(SupabaseConfig.Url, SupabaseConfig.AnonKey);

    private const string KeyConnectionString = "ConnectionString";
    private const string KeyScanDexToken = "ScanDexToken";
    private const string KeyIgdbClientId = "IgdbClientId";
    private const string KeyIgdbClientSecret = "IgdbClientSecret";
    private const string KeyRawgApiKey = "RawgApiKey";
    private const string KeyTheGamesDbApiKey = "TheGamesDbApiKey";
    private const string KeyRememberedEmail = "RememberedEmail";
    private const string KeyRememberedPassword = "RememberedPassword";

    // Modo oscuro quitado por ahora (2026-09-01): al usuario no le convenció el aspecto en
    // oscuro, así que la app se fuerza siempre a claro, ignorando el tema del sistema, hasta que
    // se retome el diseño oscuro más adelante. Los recursos AppThemeBinding "*Dark" de
    // Colors.xaml se dejan tal cual (no se usan mientras esto siga forzado, pero no hace falta
    // borrarlos para volver a esto en el futuro).
    public App()
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    // Lee la configuración guardada en SecureStorage (cifrado nativo vía Android Keystore,
    // no hace falta cifrar/descifrar a mano como con DPAPI en el escritorio). Todos los campos
    // vienen vacíos la primera vez que se abre la app en un dispositivo nuevo.
    public static async System.Threading.Tasks.Task<AppConfig> LoadConfigAsync()
    {
        return new AppConfig
        {
            ConnectionString = await SecureStorage.Default.GetAsync(KeyConnectionString) ?? string.Empty,
            ScanDexToken = await SecureStorage.Default.GetAsync(KeyScanDexToken) ?? string.Empty,
            IgdbClientId = await SecureStorage.Default.GetAsync(KeyIgdbClientId) ?? string.Empty,
            IgdbClientSecret = await SecureStorage.Default.GetAsync(KeyIgdbClientSecret) ?? string.Empty,
            RawgApiKey = await SecureStorage.Default.GetAsync(KeyRawgApiKey) ?? string.Empty,
            TheGamesDbApiKey = await SecureStorage.Default.GetAsync(KeyTheGamesDbApiKey) ?? string.Empty,
        };
    }

    // Guarda una cadena de conexión nueva y reconecta sin reiniciar la app: crea un
    // repositorio nuevo apuntando a la base indicada y lo deja como el activo (purga la
    // papelera caducada de paso, igual que hace el escritorio al conectar).
    public static async System.Threading.Tasks.Task ReconnectAsync(string connectionString)
    {
        var db = new GameDbContext(connectionString);
        var repo = new GameRepository(db);
        await repo.PurgeExpiredTrashAsync();

        Repository?.Dispose();
        Repository = repo;

        await SecureStorage.Default.SetAsync(KeyConnectionString, connectionString);
    }

    public static async System.Threading.Tasks.Task SaveApiKeysAsync(string scanDexToken, string igdbClientId,
        string igdbClientSecret, string rawgApiKey, string theGamesDbApiKey)
    {
        await SecureStorage.Default.SetAsync(KeyScanDexToken, scanDexToken);
        await SecureStorage.Default.SetAsync(KeyIgdbClientId, igdbClientId);
        await SecureStorage.Default.SetAsync(KeyIgdbClientSecret, igdbClientSecret);
        await SecureStorage.Default.SetAsync(KeyRawgApiKey, rawgApiKey);
        await SecureStorage.Default.SetAsync(KeyTheGamesDbApiKey, theGamesDbApiKey);

        ApiService = new GameApiService(scanDexToken, igdbClientId, igdbClientSecret, rawgApiKey, theGamesDbApiKey);
    }

    // Construye ApiService a partir de una config ya cargada, sin volver a escribir en
    // SecureStorage (a diferencia de SaveApiKeysAsync, pensado para cuando el usuario guarda
    // cambios de verdad desde Ajustes). Se usa al arrancar la app con una conexión ya guardada.
    public static void InitializeApiService(AppConfig config)
    {
        ApiService = new GameApiService(
            config.ScanDexToken, config.IgdbClientId, config.IgdbClientSecret,
            config.RawgApiKey, config.TheGamesDbApiKey);
    }

    public class AppConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ScanDexToken { get; set; } = string.Empty;
        public string IgdbClientId { get; set; } = string.Empty;
        public string IgdbClientSecret { get; set; } = string.Empty;
        public string RawgApiKey { get; set; } = string.Empty;
        public string TheGamesDbApiKey { get; set; } = string.Empty;
    }

    // "Recordarme" en LoginPage: guarda email+contraseña en SecureStorage (igual de cifrado que
    // el resto de esta clase) para no tener que volver a teclearlos si hace falta pasar por
    // LoginPage otra vez (sesión caducada/revocada, etc.) -- el arranque normal ya no pasa por
    // aquí gracias a la sesión persistida de Supabase.
    public static async System.Threading.Tasks.Task SaveRememberedLoginAsync(string email, string password)
    {
        await SecureStorage.Default.SetAsync(KeyRememberedEmail, email);
        await SecureStorage.Default.SetAsync(KeyRememberedPassword, password);
    }

    public static void ClearRememberedLogin()
    {
        SecureStorage.Default.Remove(KeyRememberedEmail);
        SecureStorage.Default.Remove(KeyRememberedPassword);
    }

    public static async System.Threading.Tasks.Task<RememberedLogin?> LoadRememberedLoginAsync()
    {
        var email = await SecureStorage.Default.GetAsync(KeyRememberedEmail);
        if (string.IsNullOrEmpty(email)) return null;

        var password = await SecureStorage.Default.GetAsync(KeyRememberedPassword) ?? string.Empty;
        return new RememberedLogin { Email = email, Password = password };
    }

    public class RememberedLogin
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
