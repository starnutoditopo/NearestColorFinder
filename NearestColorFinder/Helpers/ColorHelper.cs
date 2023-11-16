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

        // closed match for hues only:
        public static int GetClosestColorByHue(IEnumerable<Color> colors, Color target)
        {
            var hue1 = target.GetHue();
            var diffs = colors.Select(n => getHueDistance(n.GetHue(), hue1));
            var diffMin = diffs.Min(n => n);
            return diffs.ToList().FindIndex(n => n == diffMin);
        }

        // closed match in RGB space
        public static int GetClosestColorByRGB(IEnumerable<Color> colors, Color target)
        {
            var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
            return colors.ToList().FindIndex(n => ColorDiff(n, target) == colorDiffs);
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
        private static double getHueDistance(double hue1, double hue2)
        {
            double d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
        }

        ////  weighed only by saturation and brightness (from my trackbars)
        //public static double ColorNum(Color c)
        //{
        //    return c.GetSaturation() * factorSat +
        //                                      getBrightness(c) * factorBri;
        //}

        // distance in RGB space
        private static int ColorDiff(Color c1, Color c2)
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
