using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace VideoGameLibraryAndroid.Presentation.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : true;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : false;
    }
}
