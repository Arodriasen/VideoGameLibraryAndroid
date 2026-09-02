# Preguntas frecuentes

**¿Por qué no está en Google Play?**
Es un proyecto personal: publicarla de verdad implicaría mantener una base de datos compartida para todos los usuarios (soporte, backups, límites de uso...), y de momento no es el objetivo. Puedes instalarla igualmente descargando el APK de las [releases](https://github.com/Arodriasen/VideoGameLibraryAndroid/releases).

**¿Necesito la versión de escritorio para usar esta?**
No, funcionan de forma independiente. Si usas las dos con la misma cuenta, verás la misma colección en ambas porque comparten la base de datos.

**¿Mis datos son privados?**
Sí. Cada usuario tiene su propia base de datos (la que tú configures) y su propia cuenta, protegida por Row Level Security en Supabase — nadie más puede ver ni tus ajustes ni acceder a tu base de datos salvo que compartas tú mismo la cadena de conexión.

**¿Funciona sin conexión a internet?**
No. La colección vive en una base de datos en la nube, así que hace falta conexión para leerla y guardarla.

**¿Qué pasa si borro un juego por error?**
Se va a la papelera (icono de cubo en la barra superior) durante 7 días, con opción de restaurarlo. Pasado ese tiempo se borra para siempre.

**¿Hay modo oscuro?**
De momento no está disponible en la versión Android.

**Escaneé un código y no encontró el juego, ¿qué hago?**
Ver [Escanear códigos de barras](Escanear-codigos-de-barras): puedes buscar por nombre o rellenar el juego a mano.

**¿Puedo exportar mi colección?**
Esa función existe en la versión de escritorio (Excel/CSV); en la versión Android no está disponible — al vivir la colección en la nube y ser accesible desde el escritorio, puedes usar esa versión para exportar.
