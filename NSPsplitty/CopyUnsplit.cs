using System;
using System.IO;

namespace NSPsplitty
{
    public partial class MainWindow
    {
        private void CopyUnsplit()
        {
            try
            {
                using (var createdFile = File.Create(dir + dir.Substring(dir.LastIndexOf("\\"))))
                {
                    for(int i = 0; i < splitNumber; ++i)
                    {
                        
                        using (var indexFile = File.OpenRead(dir+string.Format("\\{0:00}",i)))
                        {
                            ConsoleText($"Starting to merge file {indexFile.Name.Substring(indexFile.Name.LastIndexOf("\\"))}.");

                            long size = 0, indexSize = new FileInfo(indexFile.Name).Length;
                            long lastSize = 0, tempSize = 0, checkMB;

                            checkMB = indexSize / 1024 / 1024;
                            byte[] buffer = new byte[chunkSize];
                            
                            while(size < indexSize)
                            {
                                createdFile.Write(buffer, 0,
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
                        }
                    }

                    createdFile.Flush();
                    createdFile.Dispose();
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
