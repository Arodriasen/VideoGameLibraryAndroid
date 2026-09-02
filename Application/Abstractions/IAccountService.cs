using System.Threading.Tasks;
using VideoGameLibraryAndroid.Application.Models;

namespace VideoGameLibraryAndroid.Application.Abstractions
{
    public enum SignUpResult { SignedIn, ConfirmationRequired }

    public interface IAccountService
    {
        Task InitializeAsync();

        // Intenta recuperar una sesión ya guardada en este dispositivo (ver
        // SecureStorageSessionPersistence en Infrastructure) -- true si hay sesión válida (o
        // refrescada) sin pedir credenciales.
        Task<bool> TryRestoreSessionAsync();

        Task<SignUpResult> SignUpAsync(string email, string password);
        Task SignInAsync(string email, string password);
        Task SignOutAsync();

        // Envía el correo de recuperación con un código (plantilla con {{ .Token }} en Supabase,
        // el enlace normal no sirve para una app móvil).
        Task RequestPasswordResetAsync(string email);

        // Verifica el código recibido por correo y, si es válido, establece la contraseña nueva.
        Task ResetPasswordAsync(string email, string code, string newPassword);

        // null si no hay ninguna fila guardada todavía (cuenta nueva) o si falla la petición
        // (sin conexión, etc.) -- el llamador decide si sigue con la caché local en ese caso.
        Task<UserSettings?> GetSettingsAsync();
        Task SaveSettingsAsync(UserSettings settings);
    }
}
