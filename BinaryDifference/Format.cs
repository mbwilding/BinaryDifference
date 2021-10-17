using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace BinaryDifference
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        public void Format()
        {
            if (Differences.Count == 0) return;
            Clear();
            switch (FormatComboBox.SelectedIndex)
            {
                case 0: // Default
                    for (int i = 0; i < Differences.Count; i++)
                    {
                        _ = ListBox1.Items.Add(FormatDefault(Differences[i].Item3, Differences[i].Item1));
                        _ = ListBox2.Items.Add(FormatDefault(Differences[i].Item3, Differences[i].Item2));
                    }
                    break;

                case 1: // Binary Patcher
                    for (int i = 0; i < Differences.Count; i++)
                    {
                        _ = ListBox1.Items.Add(FormatCSharpDictionary(Differences[i].Item3, Differences[i].Item1));
                        _ = ListBox2.Items.Add(FormatCSharpDictionary(Differences[i].Item3, Differences[i].Item2));
                    }
                    CleanCSharpDictionary(ListBox1);
                    CleanCSharpDictionary(ListBox2);
                    break;
            }
        }

        private static string FormatDefault(long offset, string hex)
        {
            return ToHex(offset) + ": " + hex;
        }

        private static string FormatCSharpDictionary(long offset, string hex)
        {
            return "{" + ToHex(offset) + ", \"" + hex + "\"},";
        }

        public static void CleanCSharpDictionary(ItemsControl listBox)
        {
            int index = listBox.Items.Count - 1;
            string content = (string)listBox.Items.GetItemAt(index);
            listBox.Items.RemoveAt(index);
            listBox.Items.Insert(index, content.Trim(new[] {','}));
        }

        private static string ToHex(long num)
        {
            return "0x" + num.ToString("X");
        }
    }
}
