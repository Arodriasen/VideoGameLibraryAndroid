using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace VideoGameLibraryAndroid.Infrastructure.Accounts
{
    // Fila de la tabla user_settings en el mismo proyecto Supabase que usa VideoGameLibrary
    // (escritorio) -- cuenta y ajustes son compartidos entre dispositivos a propósito. RLS
    // restringe cada fila a su propio usuario (auth.uid() = user_id).
    [Table("user_settings")]
    public class UserSettingsRow : BaseModel
    {
        [PrimaryKey("user_id", shouldInsert: true)]
        public Guid UserId { get; set; }

        [Column("connection_string")]
        public string ConnectionString { get; set; } = string.Empty;

        [Column("scan_dex_token")]
        public string ScanDexToken { get; set; } = string.Empty;

        [Column("igdb_client_id")]
        public string IgdbClientId { get; set; } = string.Empty;

        [Column("igdb_client_secret")]
        public string IgdbClientSecret { get; set; } = string.Empty;

        [Column("rawg_api_key")]
        public string RawgApiKey { get; set; } = string.Empty;

        [Column("thegamesdb_api_key")]
        public string TheGamesDbApiKey { get; set; } = string.Empty;
    }
}
