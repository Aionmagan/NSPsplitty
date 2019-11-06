using System;
using System.IO;

namespace NSPsplitty
{
    public partial class MainWindow
    {
        private void SplitQuick()
        {
            try
            {
                ConsoleText(string.Format(" Split is happening from file '{0:00}' to file '01'", splitNumber));

                //moving and rename original file to '00'
                Directory.Move(fileDir, dir + "\\00");

                var sizeOfSplit = fileSize - (splitSize * splitNumber);
                var splitNum = splitNumber;
                var checkMB = (sizeOfSplit / 1024 / 1024);

                //splitting starting from the last file and making it file '01' 
                //truncating file '00' on the fly
                using (var nspFile = File.Open(dir + "\\00", FileMode.Open, FileAccess.ReadWrite))
                {
                    for (int i = -1; i < splitNumber - 1; ++i)
                    {
                        if(i >= 0)
                        {
                            sizeOfSplit = splitSize;
                            splitNum = (splitNumber - (i + 1));
                            checkMB = (sizeOfSplit / 1024 / 1024);
                        }

                        nspFile.Seek(sizeOfSplit * -1, SeekOrigin.End);

                        ConsoleText(string.Format("Starting part {0:00}.", splitNum));
                        using (var splitFile = File.OpenWrite(dir + string.Format("\\{0:00}", splitNum)))
                        {
                            long size = 0, tempSize = 0, lastSize = 0;
                            byte[] buffer = new byte[chunkSize];
                            //(size = new FileInfo(splitFile.Name).Length)
                            while (size < sizeOfSplit)
                            {
                                splitFile.Write(buffer, 0,
                                                nspFile.Read(buffer, 0, buffer.Length));
                                size += chunkSize;
                                lastSize = size / 1024 / 1024;
                                if (lastSize <= tempSize) { continue; }

                                tempSize = lastSize;
                                ConsoleText($" {lastSize}MB / {checkMB}MB", true, ".");
                            }

                            splitFile.Flush();
                            splitFile.Dispose();
                        }

                        nspFile.SetLength(nspFile.Seek(sizeOfSplit * -1, SeekOrigin.End));
                        ConsoleText(string.Format(" Part {0:00} completed", splitNum));
                    }

                    nspFile.Flush();
                    nspFile.Dispose();
                }

                File.SetAttributes(dir, FileAttributes.Archive);
                ConsoleText("\n NSP split successfully");

            }catch(Exception exception)
            {
                ConsoleText(exception.Message);
                ConsoleText(exception.StackTrace);
            }
        }
    }
}
