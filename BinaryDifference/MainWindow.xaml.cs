using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ////////////////////////////// TODO DELETE AUTO CHECK
            File1_Box.Uid = @"D:\1.bin";
            File2_Box.Uid = @"D:\2.bin";
            Compare_Button.IsEnabled = true;
            Task.Run(() => CheckDifference(@"D:\1.bin", @"D:\2.bin"));
            ////////////////////////////// TODO DELETE AUTO CHECK
        }

        private void File1_Button_Click(object sender, RoutedEventArgs e)
        {
            FileBrowse(File1_Box);
        }

        private void File2_Button_Click(object sender, RoutedEventArgs e)
        {
            FileBrowse(File2_Box);
        }

        private void Compare_Button_Click(object sender, RoutedEventArgs e)
        {
            FileValidation();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - (double)e.Delta / 5);
            e.Handled = true;
        }
    }
}
