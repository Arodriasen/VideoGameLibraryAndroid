using System;

namespace VideoGameLibraryAndroid.Core;

// Extraído de Presentation/ViewModels/MainViewModel.cs a un proyecto aparte sin dependencias de
// MAUI/Android para poder testearlo con "dotnet test" normal, sin necesitar un emulador -- este
// proyecto (net9.0-android) no puede referenciarse desde un proyecto de tests net9.0 corriente
// (net9.0 no puede consumir net9.0-android), así que la lógica pura sí puede vivir en una
// librería net9.0 normal, referenciada tanto por la app como por los tests.
public static class TextListUtils
{
    // Género/Etiquetas se guardan como un único texto separado por comas (p.ej. "Acción,
    // Aventura", así lo devuelven IGDB/RAWG cuando un juego tiene varios géneros) -- se separa
    // aquí para que "Acción" filtre por igual un juego que solo tiene ese género y uno que tiene
    // "Acción, Aventura".
    public static string[] SplitGenres(string genre) =>
        genre.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    public static string[] SplitTags(string tags) =>
        tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
