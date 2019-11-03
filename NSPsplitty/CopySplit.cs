using System;
using System.IO;

namespace NSPsplitty
{
    public partial class MainWindow
    {
        private void SplitCopy()
        {
            try
            {
                var remSize = fileSize;
                using (FileStream nspfile = File.OpenRead(fileDir))
                {

                    for (int i = 0; i < splitNumber + 1; ++i)
                    {
                        ConsoleText(string.Format("Starting part {0:00}.", i));

                        var outFile = dir + string.Format("\\{0:00}", i);
                        using (FileStream splitFile = File.OpenWrite(outFile))
                        {
                            long size = 0, tempSize = 0, lastSize = 0, checkSize = splitSize;
                            byte[] buffer = new byte[chunkSize];

                            if (remSize < splitSize) { checkSize = remSize; }
                            var checkMB = checkSize / 1024 / 1024;

                            while ((size = (new FileInfo(splitFile.Name).Length)) < checkSize)
                            {
                                splitFile.Write(buffer, 0,
                                                nspfile.Read(buffer, 0, buffer.Length));

                                lastSize = size / 1024 / 1024;

                                if (lastSize <= tempSize) { continue; }

                                tempSize = lastSize;
                                ConsoleText($" {lastSize}MB / {checkMB}MB", true, ".");
                            }

                            remSize -= splitSize;

                            splitFile.Flush();
                            splitFile.Dispose();

                            ConsoleText(string.Format(" Part {0:00} completed", i));
                        }
                    }

                    nspfile.Flush();
                    nspfile.Dispose();
                }

                File.SetAttributes(dir, FileAttributes.Archive);
                ConsoleText("\n NSP splitted sccuessfully");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    Split_Button.IsEnabled = true;
                }));

            }catch(Exception exception)
            {
                ConsoleText(exception.Message);
                ConsoleText(exception.StackTrace);
            }
        }
    }
}
