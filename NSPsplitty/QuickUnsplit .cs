using System;
using System.IO;

namespace NSPsplitty
{
    public partial class MainWindow
    {
        private void QuickUnsplit()
        {
            try
            {
                //moving and rename original file to 'foldername.EXTENSION'
                if (File.Exists(dir + "\\" + dir.Substring(dir.LastIndexOf("\\"))))
                {
                    File.Delete(dir + "\\" + dir.Substring(dir.LastIndexOf("\\")));
                }

                Directory.Move(dir + "\\00", dir+"\\"+dir.Substring(dir.LastIndexOf("\\")));
                
                using (var openFile = File.Open(dir + "\\" + dir.Substring(dir.LastIndexOf("\\")), FileMode.Append, FileAccess.Write))
                {
                    for (int i = (int)splitNumber - 1; i > 0; --i)
                    {
                        long size = 0, indexSize = 0;
                        long lastSize = 0, tempSize = 0, checkMB = 0;
                        byte[] buffer = new byte[chunkSize];

                        using (var indexFile = File.OpenRead(dir + string.Format("\\{0:00}", i)))
                        {
                            ConsoleText($"Starting to merge file {indexFile.Name.Substring(indexFile.Name.LastIndexOf("\\"))}.");
                            indexSize = new FileInfo(indexFile.Name).Length;
                            checkMB = indexSize / 1024 / 1024;

                            while (size < indexSize)
                            {
                                openFile.Write(buffer, 0,
                                                indexFile.Read(buffer, 0, buffer.Length));
                                size += chunkSize;

                                lastSize = size / 1024 / 1024;

                                if (lastSize <= tempSize) { continue; }

                                tempSize = lastSize;
                                ConsoleText($" {lastSize}MB / {checkMB}MB", true, ".");
                            }

                            ConsoleText($" file {indexFile.Name.Substring(indexFile.Name.LastIndexOf("\\"))} merged completed.");

                            indexFile.Flush();
                            indexFile.Dispose();

                            File.Delete(dir+string.Format("\\{0:00}", i));
                            //ConsoleText(dir + string.Format("\\{0:00}", i));
                        }

                    }

                    openFile.Flush();
                    openFile.Dispose();
                }
                ConsoleText($"\n {dir.Substring(dir.LastIndexOf(".")).ToUpper()} merging sccuessfully");
            }
            catch (Exception exception)
            {
                ConsoleText(exception.Message);
                ConsoleText(exception.StackTrace);
            }
        }
    }
}
