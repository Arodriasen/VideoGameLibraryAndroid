# Mi Colección de Juegos (Android)

![Build](https://github.com/Arodriasen/VideoGameLibraryAndroid/actions/workflows/build.yml/badge.svg)

Aplicación para Android que permite catalogar tu colección personal de videojuegos escaneando el código de barras (UPC/EAN) de la caja con la cámara del móvil. Busca automáticamente título, plataforma, género, año y portada, y guarda todo en la misma base de datos PostgreSQL en la nube (por ejemplo [Neon](https://neon.tech), gratis) que usa la [versión de escritorio para Windows](https://github.com/Arodriasen/VideoGameLibrary) — inicia sesión con la misma cuenta en ambas y accede a tu colección desde cualquiera de los dos dispositivos.

📖 Guía completa (primeros pasos, configurar la base de datos, claves de API, preguntas frecuentes) en la [wiki del proyecto](https://github.com/Arodriasen/VideoGameLibraryAndroid/wiki).

## Características

- Escaneo del código de barras con la cámara (ML Kit), con selección entre varios candidatos cuando el código no es suficiente para identificar el juego con seguridad.
- Búsqueda manual por nombre si el escaneo no encuentra nada, también con selector de candidatos.
- Si el código escaneado ya está en tu lista de deseados, te ofrece pasarlo directamente a la colección en vez de avisarte de un duplicado.
- Colección y lista de deseados, con contador en cada una.
- Ficha del juego con portada, plataforma, editorial, género, año, etiquetas, notas, puntuación por estrellas (1 a 5, se puede quitar volviendo a tocar la misma estrella) y estado jugado / sin jugar.
- Etiquetas libres (favorito, prestado, edición coleccionista...), mostradas como chips en la ficha.
- Filtros por plataforma, género, etiqueta, año, puntuación y estado, y cuatro criterios de orden (título, plataforma, año, añadido recientemente).
- Papelera: los juegos eliminados se conservan 7 días antes de borrarse para siempre, con restauración o eliminación definitiva de uno en uno.
- Inicio de sesión con la misma cuenta que la versión de escritorio (Supabase): la colección, la cadena de conexión a la base de datos y las claves de API se comparten automáticamente entre dispositivos, sin configurarlas dos veces.
- La base de datos es tu propio proyecto en Neon (u otro Postgres compatible): nadie más tiene acceso, no hay ningún servidor propio de la app de por medio. Requiere conexión a internet — no hay modo sin conexión.

## Requisitos

- Android 5.0 (API 21) o superior.
- Cámara (opcional: sin cámara o sin permiso concedido, se puede rellenar cada juego a mano).
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) con el workload de MAUI (`dotnet workload install maui-android`) y el Android SDK, solo para compilar el proyecto (no hace falta si vas a instalar un APK ya compilado).

## Descargar

Esta app **no está en Google Play** — instala el APK directamente desde la [última release](https://github.com/Arodriasen/VideoGameLibraryAndroid/releases/latest):

1. Descarga el `.apk` de la release en tu móvil.
2. Ábrelo desde el gestor de archivos. Al no venir de Google Play, Android pedirá permiso para "instalar apps de origen desconocido" la primera vez — es normal, acéptalo solo para este archivo.
3. Al abrir la app, te pedirá crear una cuenta o iniciar sesión (ver [Primeros pasos](https://github.com/Arodriasen/VideoGameLibraryAndroid/wiki/Primeros-pasos) en la wiki).

## Compilar y ejecutar

Si prefieres compilarlo tú mismo desde el código fuente:

1. Clona el repositorio.
2. Copia `Infrastructure/Accounts/SupabaseConfig.cs.example` a `Infrastructure/Accounts/SupabaseConfig.cs` y rellena `Url`/`AnonKey` con los de **tu propio** proyecto en [supabase.com](https://supabase.com) (ese archivo no está en el repo a propósito — ver [Configurar la base de datos](https://github.com/Arodriasen/VideoGameLibraryAndroid/wiki/Configurar-la-base-de-datos) en la wiki para la tabla y las políticas RLS que necesita).
3. Compila y despliega a un dispositivo o emulador conectado:
   ```
   dotnet build VideoGameLibraryAndroid.csproj -f net9.0-android -t:Run
   ```
4. Para generar un `.apk` de Release (necesario para instalar por USB en la mayoría de dispositivos, ya que Debug usa Fast Deployment y no lleva el código incluido):
   ```
   dotnet build VideoGameLibraryAndroid.csproj -c Release -f net9.0-android
   ```
   El APK firmado queda en `bin\Release\net9.0-android\com.arodriasen.videogamelibrary-Signed.apk`.

También puedes abrir `VideoGameLibraryAndroid.sln` directamente con Visual Studio 2022 (workload ".NET Multi-platform App UI development").

## Tecnologías

.NET MAUI (.NET 9), Entity Framework Core + PostgreSQL (Npgsql), Supabase (autenticación y sincronización de ajustes), BarcodeScanning.Native.Maui (ML Kit), CommunityToolkit.Mvvm, ClosedXML.

## Licencia

Este proyecto está publicado bajo la licencia [MIT](LICENSE): puedes usarlo, modificarlo y distribuirlo libremente, incluso en proyectos privados o comerciales, siempre citando la licencia original.
