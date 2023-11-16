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
            this.Palette = new BindingList<Color>();
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
            //};
            this.LoadPalette();
            this.NamedColors = new BindingList<ColorNamePair>(ColorHelper.GetNamedColors().OrderBy(p => p.Name, StringComparer.InvariantCultureIgnoreCase).ToList());

            this.SelectedColor = this.Palette.First();
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
                    this.RaisePropertyChanged(nameof(ClosestPaletteColorRGB));
                    this.RaisePropertyChanged(nameof(ClosestPaletteColorHue));
                    this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
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

        public Color ClosestPaletteColorRGB
        {
            get
            {
                int id = ColorHelper.GetClosestColorByRGB(this.Palette, this.SelectedColor);
                return this.Palette[id];
            }
        }

        public Color ClosestPaletteColorHue
        {
            get
            {
                int id = ColorHelper.GetClosestColorByHue(this.Palette, this.SelectedColor);
                return this.Palette[id];
            }
        }

        public void CopyClosestPaletteColorRGB()
        {
            var name = colorToNameConverter.Convert(ClosestPaletteColorRGB, typeof(string), null, CultureInfo.InvariantCulture) as string;
            Clipboard.SetText(name);
        }

        private void RefillPalette(IEnumerable<Color> newItems)
        {
            var list = newItems.ToList();
            Palette.Clear();
            foreach (var item in list)
            {
                Palette.Add(item);
            }
            this.RaisePropertyChanged(nameof(Palette));
        }

        public void SortPaletteByHex()
        {
            RefillPalette(this.Palette.OrderBy(c => c.ToString(CultureInfo.InvariantCulture)));
        }

        public void SortPaletteByHue()
        {
            RefillPalette(this.Palette.OrderBy(c => c.GetHue()));
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
            }
            this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
            this.RaisePropertyChanged(nameof(Palette));
        }

        public bool CanAddSelectedColorToPalette
        {
            get
            {
                return !this.Palette.Contains(this.SelectedColor);
            }
        }
        public void AddSelectedColorToPalette()
        {
            if (CanAddSelectedColorToPalette)
            {
                this.Palette.Add(this.SelectedColor);
            }
            this.RaisePropertyChanged(nameof(CanAddSelectedColorToPalette));
            this.RaisePropertyChanged(nameof(Palette));
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