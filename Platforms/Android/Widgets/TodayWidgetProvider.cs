using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Newtonsoft.Json;
using VideoGameLibraryAndroid.Infrastructure.Logging;
using VideoGameLibraryAndroid.Infrastructure.Widgets;

namespace VideoGameLibraryAndroid.Widgets
{
    // Widget de pantalla de inicio "Juego de hoy". Vive en un proceso/contexto de Android aparte
    // de la app MAUI (RemoteViews, no el árbol de vistas de MainPage), por eso NUNCA toca
    // Neon/EF Core aquí dentro: solo lee lo que WidgetSnapshotService.Update deja en
    // SharedPreferences + los archivos de portada cacheados, cada vez que MainViewModel recarga
    // la colección real. El registro en AndroidManifest lo gestionan solos los atributos de
    // abajo, no hace falta tocar el manifest a mano.
    [BroadcastReceiver(Label = "Juego de hoy", Exported = false)]
    [IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE", ActionReroll })]
    [MetaData("android.appwidget.provider", Resource = "@xml/today_widget_info")]
    public class TodayWidgetProvider : AppWidgetProvider
    {
        public const string ActionReroll = "com.arodriasen.videogamelibrary.action.WIDGET_REROLL";

        public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
        {
            if (context == null || appWidgetManager == null || appWidgetIds == null) return;

            foreach (var id in appWidgetIds)
                PaintWidget(context, appWidgetManager, id);
        }

        // El botón 🔀 del widget dispara este broadcast en vez de abrir la app -- rotar el
        // candidato no necesita Neon, ya está cacheado por WidgetSnapshotService.
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (context != null && intent?.Action == ActionReroll)
            {
                Reroll(context);
                RequestUpdate(context);
                return;
            }
            base.OnReceive(context, intent);
        }

        // Llamado desde WidgetSnapshotService tras cada recarga real de la colección, y desde
        // aquí mismo tras un reroll -- repinta ya todas las instancias del widget en la pantalla
        // de inicio sin esperar al updatePeriodMillis (Android lo limita a un mínimo de ~30 min).
        public static void RequestUpdate(Context context)
        {
            try
            {
                var manager = AppWidgetManager.GetInstance(context)!;
                var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(TodayWidgetProvider)));
                var ids = manager.GetAppWidgetIds(component) ?? Array.Empty<int>();
                foreach (var id in ids)
                    PaintWidget(context, manager, id);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Repintar el widget Juego de hoy", ex);
            }
        }

        private static void Reroll(Context context)
        {
            var prefs = context.GetSharedPreferences(WidgetSnapshotService.PrefsName, FileCreationMode.Private);
            if (prefs == null) return;

            var candidates = ReadCandidates(prefs);
            if (candidates.Count == 0) return;

            var current = prefs.GetInt(WidgetSnapshotService.KeyCurrentIndex, 0);
            var next = (current + 1) % candidates.Count;

            var editor = prefs.Edit();
            editor?.PutInt(WidgetSnapshotService.KeyCurrentIndex, next);
            editor?.Apply();
        }

        private static List<WidgetSnapshotService.Candidate> ReadCandidates(ISharedPreferences prefs)
        {
            var json = prefs.GetString(WidgetSnapshotService.KeyCandidatesJson, null);
            if (string.IsNullOrEmpty(json)) return new List<WidgetSnapshotService.Candidate>();
            return JsonConvert.DeserializeObject<List<WidgetSnapshotService.Candidate>>(json)
                   ?? new List<WidgetSnapshotService.Candidate>();
        }

        private static void PaintWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.today_widget);
            var prefs = context.GetSharedPreferences(WidgetSnapshotService.PrefsName, FileCreationMode.Private);
            if (prefs == null) return;

            var collectionCount = prefs.GetInt(WidgetSnapshotService.KeyCollectionCount, 0);
            var wishlistCount = prefs.GetInt(WidgetSnapshotService.KeyWishlistCount, 0);
            var playedPercent = prefs.GetInt(WidgetSnapshotService.KeyPlayedPercent, 0);
            views.SetTextViewText(Resource.Id.widget_counts, $"{collectionCount} en colección · {wishlistCount} en deseados");
            views.SetTextViewText(Resource.Id.widget_progress, $"🎯 {playedPercent}% de tu colección jugada");

            var candidates = ReadCandidates(prefs);

            if (candidates.Count == 0)
            {
                views.SetTextViewText(Resource.Id.widget_title, "Sin pendientes");
                views.SetTextViewText(Resource.Id.widget_subtitle, "Tu colección está al día");
                views.SetImageViewResource(Resource.Id.widget_cover, Resource.Drawable.app_logo);
            }
            else
            {
                var index = prefs.GetInt(WidgetSnapshotService.KeyCurrentIndex, 0) % candidates.Count;
                var pick = candidates[index];

                views.SetTextViewText(Resource.Id.widget_title, pick.Title);
                views.SetTextViewText(Resource.Id.widget_subtitle,
                    string.IsNullOrEmpty(pick.YearText) ? pick.Platform : $"{pick.Platform} · {pick.YearText}");

                var coverPath = System.IO.Path.Combine(WidgetSnapshotService.CoversDirectory, $"{pick.Id}.png");
                Bitmap? bitmap = File.Exists(coverPath) ? BitmapFactory.DecodeFile(coverPath) : null;

                if (bitmap != null)
                {
                    using var rounded = RoundCorners(context, bitmap, 10);
                    bitmap.Dispose();
                    views.SetImageViewBitmap(Resource.Id.widget_cover, rounded);
                }
                else
                {
                    views.SetImageViewResource(Resource.Id.widget_cover, Resource.Drawable.app_logo);
                }
            }

            var pendingIntentFlags = OperatingSystem.IsAndroidVersionAtLeast(23)
                ? PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
                : PendingIntentFlags.UpdateCurrent;

            var openApp = PendingIntent.GetActivity(context, 0,
                new Intent(context, typeof(MainActivity)),
                pendingIntentFlags);
            if (openApp != null)
                views.SetOnClickPendingIntent(Resource.Id.widget_root, openApp);

            var rerollIntent = new Intent(context, typeof(TodayWidgetProvider));
            rerollIntent.SetAction(ActionReroll);
            var reroll = PendingIntent.GetBroadcast(context, appWidgetId, rerollIntent,
                pendingIntentFlags);
            if (reroll != null)
                views.SetOnClickPendingIntent(Resource.Id.widget_reroll, reroll);

            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }

        // RemoteViews no puede recortar un ImageView a una forma -- mismo criterio ya usado en
        // este proyecto para el icono de la app (recortar el bitmap a mano en vez de depender de
        // un control que no lo soporta), aquí con Android.Graphics.Canvas en vez de DrawingVisual.
        private static Bitmap RoundCorners(Context context, Bitmap source, float radiusDp)
        {
            var density = context.Resources!.DisplayMetrics!.Density;
            var radiusPx = radiusDp * density;

            var output = Bitmap.CreateBitmap(source.Width, source.Height, Bitmap.Config.Argb8888!)!;
            using var canvas = new Canvas(output);
            using var paint = new Android.Graphics.Paint { AntiAlias = true };
            var rect = new Android.Graphics.RectF(0, 0, source.Width, source.Height);

            canvas.DrawRoundRect(rect, radiusPx, radiusPx, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(source, 0, 0, paint);
            return output;
        }
    }
}
