using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using AdonisUI.Controls;

namespace BinaryDifference
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedMember.Global")]
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : AdonisWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Restore saved data format setting
            FormatComboBox.SelectedIndex = Properties.Settings.Default.DataFormat;

            // Sets rounded corners
            SetWindowStyle(Corner.Round);
        }

        #region Round Corners

        private void SetWindowStyle(Corner preference)
        {
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)!).EnsureHandle();
            var attribute = DwmWindowAttribute.DwnCornerPreference;
            DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, ref Corner pvAttribute, uint cbAttribute);

        public enum DwmWindowAttribute { DwnCornerPreference = 33 }

        public enum Corner
        {
            Default = 0,
            NoRound = 1,
            Round = 2,
            RoundSmall = 3
        }

        #endregion
    }
}
