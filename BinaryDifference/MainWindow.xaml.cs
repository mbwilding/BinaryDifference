using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void File1_Button_Click(object s, RoutedEventArgs e)
        {
            FileBrowse(File1_Box);
        }

        private void File2_Button_Click(object s, RoutedEventArgs e)
        {
            FileBrowse(File2_Box);
        }

        private void Save_Button_Click(object s, RoutedEventArgs e)
        {
            SaveFile();
        }
    }
}
