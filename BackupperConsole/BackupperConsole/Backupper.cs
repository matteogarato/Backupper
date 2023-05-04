using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace BackupperConsole
{
    /// <summary>
    /// Class for the backup
    /// </summary>
    public class Backupper
    {
        /// <summary>
        /// Determines whether to show output to the console
        /// </summary>
        private bool silentOp;
        /// <summary>
        /// Number of file with specific extension found
        /// </summary>
        private int FileFound = 0;

        /// <summary>
        /// Number of file with specific extension copied
        /// </summary>
        private int FileCopied = 0;

        /// <summary>
        /// Configuration
        /// </summary>
        private Configuration conf;

        public Backupper(bool silent)
        {
            conf = null;
            silentOp = silent;
        }

        /// <summary>
        /// Run the backup in terminal
        /// </summary>
        public void Run()
        {
            var initMsg = new StringBuilder("================== Welcome In The ==================\n");
            initMsg.Append("================== Backup Utility ==================\n");
            initMsg.Append("reading configuration...");
            writeLine(initMsg.ToString());
            conf = Configuration.Read();
            if (conf != null)
            {
                writeLine("configuration read...\nstarting...");
                try
                {
                    BackupRun(conf.FromDir, conf.BackupDir);
                    writeLine($"ended copy {FileCopied} of {FileFound} file");
                }
                catch (FileNotFoundException ex)
                {
                    writeLine("backup source not found");
#if DEBUG
                    writeLine(ex.Message);
#endif
                }
                writeLine("==================== End Backup ====================");
            }
            else
            {
                Configuration.CreateDefaultConfig();
                writeLine("configuration not found, created standard in Application run folder...");
            }
            writeLine("============ Press Any key To Continue =============");
            readLine();
        }

        /// <summary>
        /// Hash the file then compare byte to byte the hash
        /// </summary>
        /// <param name="filePath1">first file to compare</param>
        /// <param name="filePath2">second file to compare</param>
        /// <param name="FileName">filename</param>
        /// <returns>true if the file is the same</returns>
        private static bool Compare(string filePath1, string filePath2, string FileName)
        {
            var path = filePath2.Replace(FileName, "");
            if (!Directory.Exists(path))
            {
                return false;
            }
            if (!File.Exists(filePath2))
            {
                return false;
            }
            var hash1 = GenerateHash(filePath1);
            var hash2 = GenerateHash(filePath2);
            return hash1.SequenceEqual(hash2);
        }

        /// <summary>
        /// Generate the hash for the file
        /// </summary>
        /// <param name="filePath">path to the file</param>
        /// <returns>byte[] of the hashed file</returns>
        private static ReadOnlySpan<byte> GenerateHash(string filePath)
        {
            var crypto = MD5.Create();
            using var stream = File.OpenRead(filePath);
            return crypto.ComputeHash(stream);
        }

        /// <summary>
        /// Copy teh file
        /// </summary>
        /// <param name="source">source path</param>
        /// <param name="dest">destination path</param>
        public static void CopyFile(string source, string dest)
        {
            using var sourceStream = new FileStream(source, FileMode.Open);
            var buffer = new byte[64 * 1024];
            using var destStream = new FileStream(dest, FileMode.Create);
            int i;
            while ((i = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                destStream.Write(buffer, 0, i);
            }
        }

        /// <summary>
        /// Iterate through the folder and check file
        /// </summary>
        /// <param name="root">root source from</param>
        /// <param name="backupRoot">root of destination</param>
        public void BackupRun(string root, string backupRoot)
        {
            var dirs = new Stack<string>(20);

            if (!Directory.Exists(root))
            {
                throw new FileNotFoundException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                var currentDir = dirs.Pop();
                if (conf.IgnoreDir == null || conf.IgnoreDir?.Any(c => string.Equals(c, currentDir, StringComparison.InvariantCultureIgnoreCase)) == false)
                {
                    string[] subDirs;
                    try
                    {
                        subDirs = Directory.GetDirectories(currentDir);
                    }
                    catch (Exception ex)
                    {
                        if (ex is UnauthorizedAccessException || ex is DirectoryNotFoundException)
                        {
                            writeLine(ex.Message);
                            continue;
                        }
                        throw;
                    }
                    string[] files = null;
                    try
                    {
                        files = Directory.GetFiles(currentDir);
                    }
                    catch (Exception ex)
                    {
                        if (ex is UnauthorizedAccessException || ex is DirectoryNotFoundException)
                        {
                            writeLine(ex.Message);
                            continue;
                        }
                        throw;
                    }
                    foreach (var file in files)
                    {
                        try
                        {
                            var fi = new FileInfo(file);
                            if (conf.Extensions?.Any(c => string.Equals(c, fi.Extension, StringComparison.InvariantCultureIgnoreCase)) != false)
                            {
                                FileFound += 1;
                                var filePath = Path.Combine(fi.DirectoryName, fi.Name);
                                var backupFolder = fi.DirectoryName.Replace(root, backupRoot);
                                var backupFilePath = Path.Combine(backupFolder, fi.Name); ;
                                if (!Directory.Exists(backupFolder))
                                {
                                    Directory.CreateDirectory(backupFolder);
                                }
                                if (!Compare(filePath, backupFilePath, fi.Name))
                                {
#if DEBUG
                                    writeLine("copy: " + fi.Name);
#endif
                                    FileCopied += 1;
                                    CopyFile(filePath, backupFilePath);
                                }
                            }
                        }
                        catch (FileNotFoundException e)
                        {
                            writeLine(e.Message);
                            continue;
                        }
                    }
                    foreach (var str in subDirs)
                    {
                        dirs.Push(str);
                    }
                }
            }
        }

        private void writeLine(string msg)
        {
            if (!silentOp)
            {
                Console.WriteLine(msg);
            }
        }

        private void readLine()
        {
            if (!silentOp)
            {
#if !DEBUG
                Console.ReadKey();
#endif                
            }
        }
    }
}