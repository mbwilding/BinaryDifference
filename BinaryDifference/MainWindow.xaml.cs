﻿using System.Windows;
using System.Windows.Controls;

namespace BinaryDifference
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void File1_Button_Click(object sender, RoutedEventArgs e)
        {
            FileBrowse(CompareFile1_Box);
        }

        private void File2_Button_Click(object sender, RoutedEventArgs e)
        {
            FileBrowse(CompareFile2_Box);
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
