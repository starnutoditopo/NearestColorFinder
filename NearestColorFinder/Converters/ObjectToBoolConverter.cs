using System;
using System.Globalization;
using System.Windows.Data;

namespace NearestColorFinder.Converters
{
    internal class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                if (value == null)
                {   
                    return false;
                }
                return true;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Backward conversion i");
        }
    }
}
