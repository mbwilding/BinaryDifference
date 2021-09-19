using AdonisUI.Controls;

namespace BinaryDifference
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow : AdonisWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Restore saved data format setting
            FormatComboBox.SelectedIndex = Properties.Settings.Default.DataFormat;
        }
    }
}
