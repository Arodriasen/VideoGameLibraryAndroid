using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Newtonsoft.Json;
using VideoGameLibraryAndroid.Domain.Entities;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Widgets;

namespace VideoGameLibraryAndroid.Infrastructure.Widgets
{
    // Snapshot de datos para el widget de pantalla de inicio "Juego de hoy". Un AppWidgetProvider
    // vive en un proceso/contexto de Android aparte de la app MAUI y NO puede consultar Neon
    // directamente (ver guía oficial "How to Build Android Widgets with .NET MAUI") -- así que
    // cada vez que MainViewModel recarga la colección se deja aquí un resumen ligero
    // (SharedPreferences + portadas ya descargadas cacheadas como archivo) que el widget solo lee,
    // nunca escribe ni consulta la base de datos por su cuenta.
    public static class WidgetSnapshotService
    {
        public const string PrefsName = "today_widget_data";
        public const string KeyCandidatesJson = "candidates_json";
        public const string KeyCurrentIndex = "current_index";
        public const string KeyCollectionCount = "collection_count";
        public const string KeyWishlistCount = "wishlist_count";
        public const string KeyPlayedPercent = "played_percent";

        // Unos pocos candidatos bastan -- es lo que se puede rotar con el botón 🔀 del widget sin
        // volver a tocar Neon, y cachear la portada de cada uno ya es una descarga hecha (no una
        // nueva) porque GetAllAsync() ya trae CoverData.
        private const int MaxCandidates = 5;

        public class Candidate
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Platform { get; set; } = string.Empty;
            public string YearText { get; set; } = string.Empty;
        }

        public static string CoversDirectory =>
            Path.Combine(FileSystem.AppDataDirectory, "widget_covers");

        public static void Update(List<Game> allGames)
        {
            try
            {
                Directory.CreateDirectory(CoversDirectory);
                foreach (var file in Directory.GetFiles(CoversDirectory))
                    File.Delete(file);

                var collectionCount = allGames.Count(g => !g.IsWishlist);
                var wishlistCount = allGames.Count(g => g.IsWishlist);
                var playedCount = allGames.Count(g => !g.IsWishlist && g.Played);
                var playedPercent = collectionCount > 0 ? (int)Math.Round(100.0 * playedCount / collectionCount) : 0;

                // El "juego de hoy" solo tiene sentido sobre el backlog real: en tu colección,
                // no en deseados, y aún sin jugar.
                var backlog = allGames.Where(g => !g.IsWishlist && !g.Played).ToList();
                var random = new Random();
                var picks = backlog.OrderBy(_ => random.Next()).Take(MaxCandidates).ToList();

                var candidates = new List<Candidate>();
                foreach (var game in picks)
                {
                    candidates.Add(new Candidate
                    {
                        Id = game.Id,
                        Title = game.Title,
                        Platform = game.Platform,
                        YearText = game.Year?.ToString() ?? string.Empty
                    });

                    if (game.CoverData is { Length: > 0 })
                        File.WriteAllBytes(Path.Combine(CoversDirectory, $"{game.Id}.png"), game.CoverData);
                }

                // Totalmente cualificado a propósito: "Application" sin cualificar resolvería
                // aquí a VideoGameLibraryAndroid.Application (la capa DDD de abstracciones), no a
                // Android.App.Application -- mismo tipo de colisión de namespace ya documentada
                // para el "class App : Application" del escritorio.
                var context = global::Android.App.Application.Context;
                var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private)!;
                var editor = prefs.Edit()!;
                editor.PutString(KeyCandidatesJson, JsonConvert.SerializeObject(candidates));
                editor.PutInt(KeyCurrentIndex, 0);
                editor.PutInt(KeyCollectionCount, collectionCount);
                editor.PutInt(KeyWishlistCount, wishlistCount);
                editor.PutInt(KeyPlayedPercent, playedPercent);
                editor.Apply();

                TodayWidgetProvider.RequestUpdate(context);
            }
            catch (Exception ex)
            {
                // El widget es un extra visual -- un fallo aquí nunca debe impedir que la
                // colección se cargue con normalidad en la app.
                LoggingService.LogError("Actualizar snapshot del widget Juego de hoy", ex);
            }
        }
    }
}
