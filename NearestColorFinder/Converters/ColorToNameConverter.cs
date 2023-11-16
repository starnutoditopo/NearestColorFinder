using NearestColorFinder.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NearestColorFinder.Converters
{
    internal class ColorToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                if (value is Color)
                {
                    foreach(var p in ColorHelper.GetNamedColors())
                    {
                        if (p.Color == (Color)value)
                        {
                            return p.Name;
                        }
                    }
                    return string.Empty;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Backward conversion i");
        }
    }
}
