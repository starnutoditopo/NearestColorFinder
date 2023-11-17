using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Linq;
using Wpf.Ui.Extensions;

namespace NearestColorFinder.Helpers
{
    internal static class ColorHelper
    {
        static ColorHelper()
        {
            var colorNames = new Dictionary<Color, IList<string>>();
            var properties = typeof(Colors)
               .GetProperties(BindingFlags.Public | BindingFlags.Static)
               .Where(p => p.PropertyType == typeof(Color));
            foreach (var colorProperty in properties)
            {
                var color = (Color)colorProperty.GetValue(null);
                IList<string> names;
                if (!colorNames.TryGetValue(color, out names))
                {
                    names = new List<string>();
                    colorNames.Add(color, names);
                };
                names.Add(colorProperty.Name);
            }
            _namedColors = colorNames.AsReadOnly();
        }

        // closed match in RGB space
        public static IEnumerable<Color> GetClosestColorsByRgb(IEnumerable<Color> colors, Color target)
        {
            var result = GetMin(colors, target, GetRgbDiff);
            return result;
        }

        // closed match in HSL space
        public static IEnumerable<Color> GetClosestColorsByHsl(IEnumerable<Color> colors, Color target)
        {
            var result = GetMin(colors, target, GetHslDiff);
            return result;
        }

        private static IEnumerable<Color> GetMin(IEnumerable<Color> colors, Color target, Func<Color, Color, double> getDistance)
        {
            List<Color> minColors = new List<Color>();
            double min = double.MaxValue;
            foreach (var color in colors)
            {
                double distance = getDistance(color, target);
                if (distance < min)
                {
                    min = distance;
                    minColors.Clear();
                    minColors.Add(color);
                }
                else if (distance == min)
                {
                    minColors.Add(color);
                }
            }

            return minColors;
        }

        // distance between two hues:
        private static double GetHueDistance(double hue1, double hue2)
        {
            double d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
        }

        private static double GetHslDiff(Color c1, Color c2)
        {
            var h = GetHueDistance(c1.GetHue(), c2.GetHue());
            return Math.Sqrt(h * h
                                 + (c1.GetSaturation() - c2.GetSaturation()) * (c1.GetSaturation() - c2.GetSaturation())
                                 + (c1.GetLuminance() - c2.GetLuminance()) * (c1.GetLuminance() - c2.GetLuminance()));
        }

        // distance in RGB space
        private static double GetRgbDiff(Color c1, Color c2)
        {
            return Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                                 + (c1.G - c2.G) * (c1.G - c2.G)
                                 + (c1.B - c2.B) * (c1.B - c2.B));
        }

        private static readonly ReadOnlyDictionary<Color, IList<string>> _namedColors;

        public static IEnumerable<ColorNamePair> GetNamedColors()
        {
            foreach (var pair in _namedColors)
            {
                foreach (var name in pair.Value)
                {
                    ColorNamePair colorNamePair = new ColorNamePair(pair.Key, name);
                    yield return colorNamePair;
                }
            }
        }
    }
}
