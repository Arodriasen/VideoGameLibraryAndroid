using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using VideoGameLibraryAndroid.Infrastructure.Logging;

namespace VideoGameLibraryAndroid.Infrastructure.Accounts
{
    // Persiste la sesión de Supabase entre reinicios de la app usando SecureStorage (cifrado
    // nativo vía Android Keystore) -- equivalente móvil de FileSessionPersistence (DPAPI) en el
    // escritorio, mismo patrón documentado en la wiki "Desktop Clients" de supabase-csharp.
    //
    // Los métodos de IGotrueSessionPersistence son SÍNCRONOS por contrato del SDK, pero
    // SecureStorage de MAUI solo tiene API asíncrona (no hay alternativa síncrona). LoadSession
    // necesita el resultado ya mismo (Gotrue lo llama dentro de RetrieveSessionAsync para decidir
    // si hay sesión que restaurar), así que se envuelve en Task.Run(...).GetAwaiter().GetResult():
    // Task.Run saca la espera del contexto de sincronización de quien llama (normalmente el hilo
    // de UI durante OnAppearing) a un hilo de la pool, así que bloquear aquí no puede
    // interbloquearse con la propia UI -- solo bloquea brevemente ese hilo de la pool mientras
    // termina una lectura local de Keystore (rápida). SaveSession sí puede ser "disparar y
    // olvidar" (nadie necesita el resultado de inmediato); DestroySession usa Remove, que ya es
    // síncrono en la API de SecureStorage.
    public class SecureStorageSessionPersistence : IGotrueSessionPersistence<Session>
    {
        private const string SessionKey = "supabase_session";

        public void SaveSession(Session session)
        {
            var json = JsonConvert.SerializeObject(session);
            _ = Task.Run(async () =>
            {
                try { await SecureStorage.Default.SetAsync(SessionKey, json); }
                catch (Exception ex) { LoggingService.LogError("Guardar sesión de cuenta", ex); }
            });
        }

        public void DestroySession()
        {
            try { SecureStorage.Default.Remove(SessionKey); }
            catch (Exception ex) { LoggingService.LogError("Borrar sesión de cuenta", ex); }
        }

        public Session? LoadSession()
        {
            try
            {
                var json = Task.Run(() => SecureStorage.Default.GetAsync(SessionKey)).GetAwaiter().GetResult();
                return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<Session>(json);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Cargar sesión de cuenta", ex);
                return null;
            }
        }
    }
}
