using Avalonia.Controls;

namespace BinaryDifference
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Restore saved data format setting
            FormatComboBox.SelectedIndex = AppSettings.Load().DataFormat;
        }
    }
}
