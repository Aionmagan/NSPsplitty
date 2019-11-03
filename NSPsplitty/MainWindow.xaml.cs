using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NSPsplitty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        private string fileDir = null;
        private string dir = null;
        private long fileSize = 0;
        private long splitNumber = 0;
        private readonly uint splitSize = 0xFFFF0000;
        private readonly uint chunkSize = 0x8000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConsoleText(string str, bool rewrite = false, string indexOf = ".")
        {
            if (Thread.CurrentThread != ConsoleBox.Dispatcher.Thread)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    ConsoleText(str, rewrite, indexOf);
                }));
                return; 
            }

            if (rewrite)
            {
                ConsoleBox.Text = ConsoleBox.Text.Substring(0, ConsoleBox.Text.LastIndexOf(indexOf)+1)+"\n"+str+"\n";
            }
            else
            {
                ConsoleBox.Text += str + "\n";
            }

            ConsoleBox.Focus();
            ConsoleBox.CaretIndex = ConsoleBox.Text.Length;
            ConsoleBox.ScrollToEnd();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Multiselect = false;
            openFile.Filter = "NSP (*.nsp*)|*.nsp*";

            Nullable<bool> checkFiles = openFile.ShowDialog();

            if (checkFiles == true)
            {   
                fileDir = openFile.FileName;
                SearchBox.Text = fileDir;

                if(fileDir != null)
                {
                    //cleaning consoleText
                    ConsoleBox.Text = "";
                    Split_Button.IsEnabled = true;
                }
            }
        }

        private void Split_Click(object sender, RoutedEventArgs e)
        {
            //cleaning consoleText
            ConsoleBox.Text = "";
            Split_Button.IsEnabled = false;

            //getting drive for where the file is located (C/D ect...) 
            DriveInfo drive = new DriveInfo(fileDir[0].ToString());
            fileSize = new FileInfo(fileDir).Length;

            long tempsize = fileSize;
            if (Create_Copy.IsChecked == true) { tempsize *= 2; }

            if (tempsize > drive.TotalFreeSpace)
            {
                string str = ((float)(tempsize - drive.TotalFreeSpace) / 1024 / 1024 / 1024).ToString();
                str = str.Substring(0, str.IndexOf(".") + 3);

                ConsoleText("Not Enought Free Space \n" + str + "GB needed", false);
            }
            else
            {
                if (CalculateSplits())
                {
                    Thread thread;
                    if (Create_Copy.IsChecked == true)
                    {
                        thread = new Thread(SplitCopy);
                        thread.Priority = ThreadPriority.Highest;
                        thread.Start();
                    }
                    else
                    {
                        thread = new Thread(SplitQuick);
                        thread.Priority = ThreadPriority.Highest;
                        thread.Start();
                    }
                }
            }
        }

        private bool CalculateSplits()
        {
            ConsoleText("Calculating numbers of split files...\n");

            splitNumber = (fileSize / splitSize);

            if (splitNumber == 0)
            {
                ConsoleText("This .nsp file is under 4GB, no split is needed");

                return false;
            }

            ConsoleText($" Splitting .nsp into {splitNumber + 1} parts");

            dir = fileDir.Substring(0, fileDir.LastIndexOf(".")) + "_split.nsp";

            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            Directory.CreateDirectory(dir);

            return true;
        }


    }
}
