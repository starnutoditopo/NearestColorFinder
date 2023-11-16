using System.Windows.Media;

namespace NearestColorFinder
{
    public class ColorNamePair
    {
        public ColorNamePair(Color color, string name)
        {
            Color = color;
            Name=name;
        }
        public Color Color { get; }
        public string Name { get; }
    }
}