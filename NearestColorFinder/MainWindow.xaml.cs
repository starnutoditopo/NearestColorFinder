using System.Windows;

namespace NearestColorFinder
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = new ViewModel();
            InitializeComponent();
        }
    }
}