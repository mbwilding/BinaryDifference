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
            FileBrowse(File1Box);
        }

        private void File2_Button_Click(object s, RoutedEventArgs e)
        {
            FileBrowse(File2Box);
        }

        private void Save_Button_Click(object s, RoutedEventArgs e)
        {
            SaveFile();
        }
    }
}
