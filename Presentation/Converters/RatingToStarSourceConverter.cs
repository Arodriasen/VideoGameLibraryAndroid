using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace VideoGameLibraryAndroid.Presentation.Converters
{
    // Devuelve la imagen de la estrella en la posición ConverterParameter (1-5): rellena si la
    // puntuación (value) es igual o mayor, contorno si no. Gemelo de RatingToStarColorConverter,
    // que colorea un carácter "★" de la fuente del sistema; aquí se usan dos SVG propios para que
    // la estrella se vea idéntica en todos los Android y la vacía sea un contorno de verdad.
    // RatingToStarColorConverter se conserva: sigue usándose en otras páginas.
    public class RatingToStarSourceConverter : IValueConverter
    {
        // Cacheadas: la lista crea cinco Image por tarjeta, y esto evita reconstruir el
        // ImageSource en cada pasada del CollectionView al desplazar.
        private static readonly ImageSource Filled = ImageSource.FromFile("icon_star.svg");
        private static readonly ImageSource Outline = ImageSource.FromFile("icon_star_outline.svg");

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var rating = value is int i ? i : 0;
            var position = parameter is string s && int.TryParse(s, out var p) ? p : 0;
            return rating >= position ? Filled : Outline;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
