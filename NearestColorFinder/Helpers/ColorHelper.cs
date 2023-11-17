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
        public static int GetClosestColorByRgb(IEnumerable<Color> colors, Color target)
        {
            var colorDiffs = colors.Select(n => GetRgbDiff(n, target)).Min(n => n);
            var result = colors.ToList().FindIndex(n => GetRgbDiff(n, target) == colorDiffs);
            return result;
        }

        // closed match in HSL space
        public static int GetClosestColorByHsl(IEnumerable<Color> colors, Color target)
        {
            var colorDiffs = colors.Select(n => GetHslDiff(n, target)).Min(n => n);
            return colors.ToList().FindIndex(n => GetHslDiff(n, target) == colorDiffs);
        }

        //// weighed distance using hue, saturation and brightness
        //public static int closestColor3(List<Color> colors, Color target)
        //{
        //    double hue1 = target.GetHue();
        //    var num1 = ColorNum(target);
        //    var diffs = colors.Select(n => Math.Abs(ColorNum(n) - num1) +
        //                                   getHueDistance(n.GetHue(), hue1));
        //    var diffMin = diffs.Min(x => x);
        //    return diffs.ToList().FindIndex(n => n == diffMin);
        //}

        // color brightness as perceived:
        //public static double getBrightness(Color c)
        //{ return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }

        // distance between two hues:
        private static double GetHueDistance(double hue1, double hue2)
        {
            double d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
        }

        private static int GetHslDiff(Color c1, Color c2)
        {
            var h = GetHueDistance(c1.GetHue(), c2.GetHue());
            return (int)Math.Sqrt(h * h
                                 + (c1.GetSaturation() - c2.GetSaturation()) * (c1.GetSaturation() - c2.GetSaturation())
                                 + (c1.GetLuminance() - c2.GetLuminance()) * (c1.GetLuminance() - c2.GetLuminance()));
        }

        // distance in RGB space
        private static int GetRgbDiff(Color c1, Color c2)
        {
            return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
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
