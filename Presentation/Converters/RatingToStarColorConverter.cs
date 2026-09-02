using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace VideoGameLibraryAndroid.Presentation.Converters
{
    // Colorea la estrella en la posición ConverterParameter (1-5) de ámbar si la puntuación
    // (value) es igual o mayor, o gris si no -- misma idea que RatingToStarKindConverter del
    // escritorio, pero coloreando un carácter "★" fijo en vez de cambiar de icono (MAUI no trae
    // la fuente de iconos de Material Design que sí tiene MaterialDesignThemes en WPF).
    public class RatingToStarColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var rating = value is int i ? i : 0;
            var position = parameter is string s && int.TryParse(s, out var p) ? p : 0;
            return rating >= position ? Color.FromArgb("#FFC107") : Color.FromArgb("#B0B0B0");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
