# Escanear códigos de barras

Al pulsar **+** → "Escanear código de barras", la app pide permiso de cámara la primera vez (puedes revocarlo luego desde los ajustes de Android; sin permiso, siempre puedes rellenar el juego a mano). Apunta al código UPC/EAN de la caja o el disco hasta que se detecte solo — no hace falta pulsar nada, vibra al reconocerlo.

## Qué pasa después de escanear

- **El código no está en tu colección ni en deseados**: la app busca el juego automáticamente (ver [Claves de API](Claves-de-API) para mejorar los resultados) y rellena el formulario de alta. Si encuentra varios candidatos posibles, te deja elegir cuál es de una lista con portada, título, plataforma y año.
- **El código ya está en tu lista de deseados**: te pregunta si quieres pasarlo a tu colección en vez de darlo de alta otra vez.
- **El código ya está en tu colección**: te avisa de que ya lo tienes.

## Si no encuentra el juego

Ningún servicio de código de barras es infalible, sobre todo con ediciones antiguas, regionales o de coleccionista. Si el escaneo no encuentra nada:

- Usa la lupa junto al campo Título del formulario para buscar por nombre en su lugar (hasta 25 resultados, ordenados del más nuevo al más antiguo).
- O simplemente rellena Título, Plataforma, Editorial, Género, Año, etc. a mano — la ficha queda igual de completa.
