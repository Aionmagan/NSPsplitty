using System;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Windows;

namespace NSPsplitty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        private string fileDir = null;
        private string dir = null;
        private string dotExtension = null;
        private bool mergeFile = false;
        private long fileSize = 0;
        private long splitNumber = 0;
        private readonly uint splitSize = 0xFFFF0000;
        private readonly uint chunkSize = 0x8000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MergeState(bool merge)
        {
            if (merge == true)
            { 
                SearchBox.Text = "Browse for the file you will like to Merge";
                Split_Button.Content = "Merge files";
            } else
            {
                SearchBox.Text = "Browse for the file you will like to split";
                Split_Button.Content = "Split File";
            }
            //Split_Button.IsEnabled = false;
            mergeFile = merge;
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
            openFile.Filter = "NSP(*.nsp*)|*.nsp*| All files(*.*)|*.*";

            Nullable<bool> checkFile = openFile.ShowDialog();
            MergeState(false);
            
            if (checkFile == true)
            {   
                fileDir = openFile.FileName;

                /*better way to check for all 'xx'(01, 02,...) or equal files 
                 * by looking for a missing (.) at the end
                 * will apply later for testing 
                 * for now it will require file '00' to enable merge 
                */
                if (fileDir.Substring(fileDir.LastIndexOf("\\")) == "\\00")
                {
                    dir = fileDir.Substring(0, fileDir.LastIndexOf("\\"));
                    MergeState(true);
                }
                
                SearchBox.Text = fileDir;

                if(fileDir != null)
                {
                    //cleaning consoleText
                    ConsoleBox.Text = "";
                    //ConsoleText($"file extension is {dotExtension}");
                    splitNumber = 0;
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

            //double code, need to optimize, works for now 
            if (mergeFile)
            {
                //starting a 1 because 00 was already apply 6 above this one
                int i = 1;
                while (File.Exists(dir + string.Format("\\{0:00}", i)))
                {
                    fileSize += new FileInfo(dir + string.Format("\\{0:00}", i)).Length;
                    i++;
                }
            }

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

                Thread thread;
                if (Create_Copy.IsChecked == true)
                {
                    if (mergeFile)
                    {
                        if (CalculateUnsplit())
                        {
                            thread = new Thread(CopyUnsplit);
                            thread.Priority = ThreadPriority.Highest;
                            thread.Start();
                        }
                    }
                    else 
                    {
                        if (CalculateSplits())
                        {
                            thread = new Thread(SplitCopy);
                            thread.Priority = ThreadPriority.Highest;
                            thread.Start();
                        }
                    }
                    
                }
                else
                {
                    if (mergeFile)
                    {
                        if (CalculateUnsplit())
                        {
                            thread = new Thread(QuickUnsplit);
                            thread.Priority = ThreadPriority.Highest;
                            thread.Start();
                        }
                    }
                    else 
                    {
                        if (CalculateSplits())
                        {
                            thread = new Thread(SplitQuick);
                            thread.Priority = ThreadPriority.Highest;
                            thread.Start();
                        }
                    }
                }

            }
        }

        private bool CalculateUnsplit()
        {
            //checking for amounts of files '0x'
            do
            {
                splitNumber++;

            } while (File.Exists(dir + string.Format("\\{0:00}", splitNumber)));

            ConsoleText($"folder contains {splitNumber} mergeable files");


            if (splitNumber <= 1)
            {
                ConsoleText($"{splitNumber} is not enough to merge files");
                return false;
            }

            return true;
        }

        private bool CalculateSplits()
        {
            ConsoleText("Calculating numbers of split files...\n");

            splitNumber = (fileSize / splitSize);

            if (splitNumber == 0)
            {
                ConsoleText($"This {dotExtension} file is under 4GB, no split is needed");

                return false;
            }

            dotExtension = fileDir.Substring(fileDir.LastIndexOf("."));
            ConsoleText($" Splitting {dotExtension} into {splitNumber + 1} parts");
            dir = fileDir.Substring(0, fileDir.LastIndexOf(".")) + "_split"+dotExtension;

            if (Directory.Exists(dir)) { Directory.Delete(dir, true); }
            Directory.CreateDirectory(dir);

            return true;
        }


    }
}
