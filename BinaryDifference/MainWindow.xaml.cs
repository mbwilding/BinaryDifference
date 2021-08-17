using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

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
            if (CompareFile1_Box.Uid != string.Empty && CompareFile2_Box.Uid != string.Empty)
            {   //TODO Add thread
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
                    Compare_Listbox1.Items.Clear();
                    Compare_Listbox2.Items.Clear();
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

        private void ItemEdit(ListBox box, int index, string text)
        {
            string content = (String)box.Items.GetItemAt(index);
            box.Items.RemoveAt(index);
            box.Items.Insert(index, content + text);
        }

        private void CheckDifference(string file1, string file2)
        {
            ButtonToggle();
            CompareStatus_Box.Text = "Processing...";
            int offset = 0;
            int index = 0;
            bool sequentialDiff = false;
            for (int i = 0; i < file1.Length / 2; i++)  //TODO Change code to accept different file sizes
            {
                string temp1 = file1.Substring(offset * 2, 2);
                string temp2 = file2.Substring(offset * 2, 2);
                if (temp1 != temp2)
                {
                    if (!sequentialDiff)
                    {
                        string box1 = "0x" + offset.ToString("X") + ": " + temp1;
                        string box2 = "0x" + offset.ToString("X") + ": " + temp2;
                        index = Compare_Listbox1.Items.Add(box1);
                        Compare_Listbox2.Items.Add(box2);
                        sequentialDiff = true;
                    }
                    else
                    {
                        ItemEdit(Compare_Listbox1, index, temp1);
                        ItemEdit(Compare_Listbox2, index, temp2);
                    }
                }
                else
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
