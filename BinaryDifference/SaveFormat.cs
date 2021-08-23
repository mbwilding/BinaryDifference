using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        public int SaveFormat(long fileOffset, int bufferOffset, string hex1, string hex2, int index, bool init, string format)
        {
            switch (format)
            {
                case "Default":
                    if (init)
                    {
                        index = ListBox1.Items.Add(StringPrepareDefault(fileOffset, bufferOffset, hex1));
                        ListBox2.Items.Add(StringPrepareDefault(fileOffset, bufferOffset, hex2));
                    }
                    else
                    {
                        ItemEditDefault(ListBox1, index, hex1);
                        ItemEditDefault(ListBox2, index, hex2);
                    }
                    break;

                case "C# Dictionary":
                    if (init)
                    {
                        index = ListBox1.Items.Add(StringPrepareCSharpDictionary(fileOffset, bufferOffset, hex1));
                        ListBox2.Items.Add(StringPrepareCSharpDictionary(fileOffset, bufferOffset, hex2));
                    }
                    else
                    {
                        ItemEditCSharpDictionary(ListBox1, index, hex1);
                        ItemEditCSharpDictionary(ListBox2, index, hex2);
                    }
                    break;
            }
            return index;
        }
        
        private static string StringPrepareDefault(long fileOffset, int bufferOffset, string value)
        {
            return "0x" + (fileOffset + bufferOffset).ToString("X") + ": " + value;
        }
        private static void ItemEditDefault(ItemsControl listBox, int index, string append)
        {
            string content = (String)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content + append);
        }

        private static string StringPrepareCSharpDictionary(long fileOffset, int bufferOffset, string value)
        {
            // {0x112EC6D, "E9380200"},
            return "{0x" + (fileOffset + bufferOffset).ToString("X") + ", \"" + value + "\"},";
        }

        private static void ItemEditCSharpDictionary(ItemsControl listBox, int index, string append)
        {
            string content = (String)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content.Insert(content.Length - 3, append));
        }

        public static void ItemCleanCSharpDictionary(ItemsControl listBox, int index)
        {
            string content = (string)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content.Trim(new[] { ','}));
        }
    }
}
