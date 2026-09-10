namespace VideoGameLibraryAndroid.Application.Models
{
    // Los mismos 6 campos que App.AppConfig, pero vistos desde la cuenta (Supabase) en vez del
    // caché local (SecureStorage) -- es el DTO que cruza la frontera de IAccountService. Mismo
    // tipo que el equivalente en VideoGameLibrary (escritorio), que usa la misma cuenta/tabla.
    public class UserSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ScanDexToken { get; set; } = string.Empty;
        public string IgdbClientId { get; set; } = string.Empty;
        public string IgdbClientSecret { get; set; } = string.Empty;
        public string RawgApiKey { get; set; } = string.Empty;
        public string TheGamesDbApiKey { get; set; } = string.Empty;
    }
}
