# Claves de API (opcionales)

La app funciona sin ninguna clave configurada, pero el escaneo por código de barras acierta más y trae mejores datos (portada, género, plataforma) cuantas más claves añadas. Todas son gratuitas.

| Servicio | Para qué se usa | Dónde conseguirla |
|---|---|---|
| [ScanDex](https://scandex.gamery.app/) | Resolución del código de barras específica de videojuegos (primer intento) | Web de ScanDex |
| [UPCitemdb](https://www.upcitemdb.com/) | Resolución del código de barras genérica, sin registro | No necesita clave |
| [IGDB](https://api-docs.igdb.com/) | Enriquecimiento por nombre (portada, género, plataforma) | Client ID y Secret desde una app registrada en la [consola de desarrolladores de Twitch](https://dev.twitch.tv/console/apps) |
| [RAWG](https://rawg.io/apidocs) | Enriquecimiento por nombre, segunda fuente | Clave gratuita en rawg.io |
| [TheGamesDB](https://thegamesdb.net/) | Portada como último recurso | Clave gratuita solicitándola en su foro |

## Cómo añadirlas

Desde la app: icono de engranaje (Ajustes) → pega cada clave en su campo → GUARDAR. Se sincronizan automáticamente con el resto de tus dispositivos (misma cuenta).

Puedes omitir cualquiera de ellas: si falta una clave, esa fuente simplemente se salta sin romper nada — el escaneo sigue funcionando con las que sí tengas configuradas.
