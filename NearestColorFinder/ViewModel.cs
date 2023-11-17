using NearestColorFinder.Converters;
using NearestColorFinder.Helpers;
using NearestColorFinder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Extensions;

namespace NearestColorFinder
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly ColorToNameConverter colorToNameConverter = new ColorToNameConverter();

        public ViewModel()
        {
            this.Palette = new BindingList<Color>()
            //{
            //    Colors.LightBlue,
            //    Colors.LightCoral,
            //    Colors.LightCyan,
            //    Colors.LightGoldenrodYellow,
            //    Colors.LightGray,
            //    Colors.LightGreen,
            //    Colors.LightPink,
            //    Colors.LightSalmon,
            //    Colors.LightSeaGreen,
            //    Colors.LightSkyBlue,
            //    Colors.LightSlateGray,
            //    Colors.LightSteelBlue,
            //    Colors.LightYellow
            //}
            ;
            this.LoadPalette();
            this.NamedColors = new BindingList<ColorNamePair>(ColorHelper.GetNamedColors().OrderBy(p => p.Name, StringComparer.InvariantCultureIgnoreCase).ToList());

            this.SelectedColor = this.Palette.Any() ? this.Palette.First() : Colors.Black;
        }

        public BindingList<ColorNamePair> NamedColors { get; }
        public BindingList<Color> Palette { get; }

        private Color _selectedColor;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Color SelectedColor
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                if (value != _selectedColor)
                {
                    this._selectedColor = value;
                    this.RaisePropertyChanged(nameof(SelectedColor));
                    this.RaisePropertyChanged(nameof(ClosestPaletteColorRgb));
                    this.RaisePropertyChanged(nameof(ClosestPaletteColorHsl));
                    this.RaisePropertyChanged(nameof(ClosestNamedColorRgb));
                    this.RaisePropertyChanged(nameof(ClosestNamedColorHsl));
                    this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
                    this.RaisePropertyChanged(nameof(CanAddClosestNamedColorRgbToPalette));
                }
            }
        }

        public void SetSelectedColor(SelectionChangedEventArgs e)
        {
            foreach (object o in e.AddedItems)
            {
                if (o is Color)
                {
                    SelectedColor = (Color)o;
                    return;
                }
            }
        }

        public void SetSelectedNamedColor(SelectionChangedEventArgs e)
        {
            foreach (object o in e.AddedItems)
            {
                if (o is ColorNamePair)
                {
                    SelectedColor = ((ColorNamePair)o).Color;
                    return;
                }
            }
        }

        public Color ClosestPaletteColorRgb
        {
            get
            {
                int id = ColorHelper.GetClosestColorByRgb(this.Palette, this.SelectedColor);
                return this.Palette[id];
            }
        }
        public Color ClosestNamedColorRgb
        {
            get
            {
                int id = ColorHelper.GetClosestColorByRgb(this.NamedColors.Select(p=>p.Color), this.SelectedColor);
                return this.NamedColors[id].Color;
            }
        }
        

        public Color ClosestPaletteColorHsl
        {
            get
            {
                int id = ColorHelper.GetClosestColorByHsl(this.Palette, this.SelectedColor);
                return this.Palette[id];
            }
        }
        public Color ClosestNamedColorHsl
        {
            get
            {
                int id = ColorHelper.GetClosestColorByHsl(this.NamedColors.Select(p => p.Color), this.SelectedColor);
                return this.NamedColors[id].Color;
            }
        }

        public void CopyClosestPaletteColorRgb()
        {
            var name = colorToNameConverter.Convert(ClosestPaletteColorRgb, typeof(string), null, CultureInfo.InvariantCulture) as string;
            Clipboard.SetText(name);
        }
        public void CopyClosestNamedColorRgb()
        {
            var name = colorToNameConverter.Convert(ClosestNamedColorRgb, typeof(string), null, CultureInfo.InvariantCulture) as string;
            Clipboard.SetText(name);
        }

        private void RefillPalette(IEnumerable<Color> newItems)
        {
            if (newItems != null)
            {
                var list = newItems.ToList();
                Palette.Clear();
                foreach (var item in list)
                {
                    Palette.Add(item);
                }
                this.RaisePropertyChanged(nameof(Palette));
            }
        }

        public void SortPaletteByRgb()
        {
            RefillPalette(this.Palette.OrderBy(c => c.R).ThenBy(c => c.G).ThenBy(c => c.B));
        }

        public void SortPaletteByHsl()
        {
            RefillPalette(this.Palette.OrderBy(c => c.GetHue()).ThenBy(c => c.GetSaturation()).ThenBy(c => c.GetLuminance()));
        }

        public void SortPaletteByName()
        {
            RefillPalette(this.Palette.OrderBy(c => colorToNameConverter.Convert(c, typeof(string), null, CultureInfo.InvariantCulture) as string));
        }

        public void RemoveFromPalette(object o)
        {
            if (o is Color)
            {
                this.Palette.Remove((Color)o);
                this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
                this.RaisePropertyChanged(nameof(CanAddClosestNamedColorRgbToPalette));
                this.RaisePropertyChanged(nameof(Palette));
            }
        }

        public bool CanAddSelectedColorToPalette
        {
            get
            {
                return !this.Palette.Contains(this.SelectedColor);
            }
        }
        public bool CanAddClosestNamedColorRgbToPalette
        {
            get
            {
                return !this.Palette.Contains(this.ClosestNamedColorRgb);
            }
        }

        public void AddSelectedColorToPalette()
        {
            if (CanAddSelectedColorToPalette)
            {
                this.Palette.Add(this.SelectedColor);
                this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
                this.RaisePropertyChanged(nameof(CanAddClosestNamedColorRgbToPalette));
                this.RaisePropertyChanged(nameof(Palette));
            }
        }

        public void AddClosestNamedColorRgbToPalette()
        {
            if (CanAddClosestNamedColorRgbToPalette)
            {
                this.Palette.Add(this.SelectedColor);
                this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
                this.RaisePropertyChanged(nameof(CanAddClosestNamedColorRgbToPalette));
                this.RaisePropertyChanged(nameof(Palette));
            }
        }

        public void SavePalette()
        {
            Settings.Default.Palette = this.Palette.ToArray();
            Settings.Default.Save();
        }

        public void LoadPalette()
        {
            Settings.Default.Reload();
            RefillPalette(Settings.Default.Palette);
        }
    }
}