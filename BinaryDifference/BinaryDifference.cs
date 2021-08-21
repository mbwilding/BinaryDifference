using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BinaryDifference
{
    public partial class MainWindow
    {
        private void CheckDifference(string filePath1, string filePath2)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Dispatcher.Invoke(new ThreadStart(() =>
                {
                    ButtonToggle();
                    Listbox1.Items.Clear();
                    Listbox2.Items.Clear();
                    Status_Box.Text = "Processing...";
                }
            ));


            var fileStream1 = new FileManager(filePath1);
            var fileStream2 = new FileManager(filePath2);

            // for loop eventually
            var thread1 = new ThreadWorker();
            Task<Tuple<List<string>, List<string>>> task1 = Task.Factory.StartNew(() => thread1.ThreadProcess(fileStream1, fileStream2, fileStream1.BufferSize * 0));
            Thread.Sleep(100);
            var thread2 = new ThreadWorker();
            Task<Tuple<List<string>, List<string>>> task2 = Task.Factory.StartNew(() => thread2.ThreadProcess(fileStream1, fileStream2, fileStream1.BufferSize * 1));
            Thread.Sleep(100);
            var thread3 = new ThreadWorker();
            Task<Tuple<List<string>, List<string>>> task3 = Task.Factory.StartNew(() => thread3.ThreadProcess(fileStream1, fileStream2, fileStream1.BufferSize * 2));
            Thread.Sleep(100);
            var thread4 = new ThreadWorker();
            Task<Tuple<List<string>, List<string>>> task4 = Task.Factory.StartNew(() => thread4.ThreadProcess(fileStream1, fileStream2, fileStream1.BufferSize * 3));
            Thread.Sleep(100);
            Task.WaitAll(task1, task2, task3, task4);


            var finalList1 = task1.Result.Item1
                .Concat(task2.Result.Item1)
                .Concat(task3.Result.Item1)
                .Concat(task4.Result.Item1)
                .ToList();
            var finalList2 = task1.Result.Item2
                .Concat(task2.Result.Item2)
                .Concat(task3.Result.Item2)
                .Concat(task4.Result.Item2)
                .ToList();

            Dispatcher.Invoke(new ThreadStart(() =>
                {
                    foreach (string s in finalList1)
                    {
                        Listbox1.Items.Add(s);
                    }
                    foreach (string s in finalList2)
                    {
                        Listbox2.Items.Add(s);
                    }
                }
            ));

            if (Listbox1.Items.IsEmpty)
            {
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        ButtonToggle();
                        Status_Box.Text = "Files are identical. Time elapsed: " + ElapsedTime(stopWatch);
                    }
                ));
            }
            else
            {
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        ButtonToggle();
                        Save_Button.IsEnabled = true;
                        Status_Box.Text = "Compare completed. Time elapsed: " + ElapsedTime(stopWatch);
                    }
                ));
            }
        }
    }
}