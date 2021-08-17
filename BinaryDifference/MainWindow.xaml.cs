using System;
using System.IO;
using System.Threading;
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
            FileValidation();
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

                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            Compare_Listbox1.Items.Clear();
                            Compare_Listbox2.Items.Clear();
                        }
                    ));
                }
            }
        }

        private void FileValidation()
        {
            if (CompareFile1_Box.Uid != string.Empty && CompareFile2_Box.Uid != string.Empty)
            {
                FileInfo file1 = new FileInfo(CompareFile1_Box.Uid);
                FileInfo file2 = new FileInfo(CompareFile2_Box.Uid);

                if (file1.Length == file2.Length)
                {
                    CheckDifference(LoadFile(CompareFile1_Box.Uid), LoadFile(CompareFile2_Box.Uid));
                }
                else
                {
                    CompareStatus_Box.Text = "Files cannot be different sizes.";
                }
            }
            else
            {
                CompareStatus_Box.Text = "Load both files first.";
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
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                File1_Button.IsEnabled = !File1_Button.IsEnabled;
                File2_Button.IsEnabled = !File2_Button.IsEnabled;
                Compare_Button.IsEnabled = !Compare_Button.IsEnabled;
            }
            ));
        }

        private void ItemEdit(ListBox box, int index, string text)
        {
            string content = (String)box.Items.GetItemAt(index);
            box.Items.RemoveAt(index);
            box.Items.Insert(index, content + text);
        }

        private void CheckDifference(string file1, string file2)
        {
            new Thread(() =>
            {
                ButtonToggle();
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        Compare_Listbox1.Items.Clear();
                        Compare_Listbox2.Items.Clear();
                        CompareStatus_Box.Text = "Processing...";
                    }
                ));
                int offset = 0;
                int index = 0;
                bool sequentialDiff = false;
                for (int i = 0; i < file1.Length / 2; i++)
                {
                    string temp1 = file1.Substring(offset * 2, 2);
                    string temp2 = file2.Substring(offset * 2, 2);
                    if (temp1 != temp2)
                    {
                        if (!sequentialDiff)
                        {
                            sequentialDiff = true;
                            string box1 = "0x" + offset.ToString("X") + ": " + temp1;
                            string box2 = "0x" + offset.ToString("X") + ": " + temp2;
                            
                            Dispatcher.BeginInvoke(new ThreadStart(() =>
                                {
                                    index = Compare_Listbox1.Items.Add(box1);
                                    Compare_Listbox2.Items.Add(box2);
                                }
                            ));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new ThreadStart(() =>
                                {
                                    ItemEdit(Compare_Listbox1, index, temp1);
                                    ItemEdit(Compare_Listbox2, index, temp2);
                                }
                            ));
                        }
                    }
                    else
                    {
                        sequentialDiff = false;
                    }
                    offset++;
                }

                Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        ButtonToggle();
                        CompareStatus_Box.Text = "Compare completed.";
                    }
                ));

            }).Start();
        }
    }
}
