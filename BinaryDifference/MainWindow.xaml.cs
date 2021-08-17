using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace BinaryDifference
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
            if (CompareFile1_Box.Uid != String.Empty && CompareFile2_Box.Uid != String.Empty)
            {
                /*
                this.Dispatcher.Invoke(() =>
                {
                    CheckDifference(LoadFile(CompareFile1_Box.Uid), LoadFile(CompareFile2_Box.Uid));
                });
                */
                CheckDifference(LoadFile(CompareFile1_Box.Uid), LoadFile(CompareFile2_Box.Uid));
            }
            else
            {
                CompareStatus_Box.Text = "Load both files first.";
            }
        }

        private void FileBrowse(TextBox box)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                var size = new FileInfo(fileDialog.FileName).Length;
                if (size > 0x7FFFFFFF)
                {
                    CompareStatus_Box.Text = "Files have to be under 2GB.";
                }
                else
                {
                    box.Text = fileDialog.SafeFileName;
                    box.Uid = fileDialog.FileName;
                    CompareStatus_Box.Text = "File loaded.";
                }
            }
        }

        private static string LoadFile(string filePath)
        {
            return ConvertBinaryToHex(File.ReadAllBytes(filePath));
        }

        private static string ConvertBinaryToHex(byte[] binaryFile)
        {
            return BitConverter.ToString(binaryFile).Replace("-", string.Empty);
        }

        private void ButtonToggle()
        {
            File1_Button.IsEnabled = !File1_Button.IsEnabled;
            File2_Button.IsEnabled = !File2_Button.IsEnabled;
            Compare_Button.IsEnabled = !Compare_Button.IsEnabled;
        }

        private void CheckDifference(string file1, string file2)
        {
            ButtonToggle();
            Compare_Scroll1.Content = String.Empty;
            Compare_Scroll2.Content = String.Empty;
            CompareStatus_Box.Text = "Processing...";

            int offset = 0;
            bool sequentialDiff = false;
            int length = file1.Length / 2;      // TODO Fix code for different size files
            for (int i = 0; i < length; i++)
            {
                string temp1 = file1.Substring(offset * 2, 2);
                string temp2 = file2.Substring(offset * 2, 2);
                if (temp1 != temp2)
                {
                    if (!sequentialDiff)
                    {
                        Compare_Scroll1.Content += "\r\n0x" + offset.ToString("X") + ": ";
                        Compare_Scroll2.Content += "\r\n0x" + offset.ToString("X") + ": ";
                        sequentialDiff = true;
                    }

                    Compare_Scroll1.Content += temp2;
                    Compare_Scroll2.Content += temp1;
                }
                else // Same value
                {
                    sequentialDiff = false;
                }

                offset++;
            }
            ButtonToggle();
            CompareStatus_Box.Text = "Compare completed.";
        }
    }
}
