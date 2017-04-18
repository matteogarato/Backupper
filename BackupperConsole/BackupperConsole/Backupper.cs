using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
namespace BackupperConsole
{
    /// <summary>
    /// class for the backup
    /// </summary>
    public class Backupper
    {
        /// <summary>
        /// number of file with specific extension found
        /// </summary>
        private int FileFound = 0;
        /// <summary>
        /// number of file with specific extension copied 
        /// </summary>
        private int FileCopied = 0;
        /// <summary>
        /// configuration
        /// </summary>
        private Configuration conf;
        /// <summary>
        /// run the backup in terminal
        /// </summary>
        public void Run()
        {
            Console.WriteLine("================== Welcome In The ==================");
            Console.WriteLine("================== Backup Utility ==================");
            Console.WriteLine("reading configuration...");
            conf = new Configuration();
            if (conf.ExistConfig())
            {
                conf.ReadConfigurationSafe();
                Console.WriteLine("configuration readed...");
                Console.WriteLine("starting...");
                BackupRun(conf.FromDir, conf.BackupDir);
                Console.WriteLine(string.Format("ended copy {0} of {1} file", FileCopied, FileFound));
                Console.WriteLine("==================== End Backup ====================");
            }
            else
            {
                conf.ReadConfigurationSafe();
                Console.WriteLine(string.Format("configuration not found, created standard in {0}...", conf.Path));
            }
            Console.WriteLine("============ Press Any key To Continue =============");
            Console.ReadLine();
        }
        /// <summary>
        /// hash the file then compare byte to byte the hash
        /// </summary>
        /// <param name="filePath1">first file to compare</param>
        /// <param name="filePath2">second file to compare</param>
        /// <param name="FileName">filename</param>
        /// <returns>true if the file is the same</returns>
        private bool Compare(string filePath1, string filePath2, string FileName)
        {
            var path = filePath2.Replace(FileName, "");
            if (!Directory.Exists(path))
            {
                NormalizeFolder(path);
                return false;
            }
            if (!File.Exists(filePath2))
            {
                return false;
            }

            byte[] hash1 = GenerateHash(filePath1);
            byte[] hash2 = GenerateHash(filePath2);

            if (hash1.Length != hash2.Length)
            {
                return false;
            }
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// generate the hash for the file
        /// </summary>
        /// <param name="filePath">path to the file</param>
        /// <returns>byte[] of the hashed file</returns>
        private byte[] GenerateHash(string filePath)
        {
            MD5 crypto = MD5.Create();
            using (FileStream stream = File.OpenRead(filePath))
            {
                return crypto.ComputeHash(stream);
            }
        }
        /// <summary>
        /// check the existance of the path to the file, if not found it will be created
        /// </summary>
        /// <param name="PathFile">path to file</param>
        private void NormalizeFolder(string PathFile)
        {
            string[] paths = PathFile.Split(Path.DirectorySeparatorChar);
            string pathToCheck = "";
            foreach (var path in paths)
            {
                if (path != null && !string.IsNullOrEmpty(path))
                {
                    pathToCheck = Path.Combine(pathToCheck, path);
                    if (!Directory.Exists(pathToCheck))
                    {
                        Directory.CreateDirectory(pathToCheck);
                    }
                }
            }

        }
        /// <summary>
        /// copy teh file
        /// </summary>
        /// <param name="source">source path</param>
        /// <param name="dest">destination path</param>
        public void CopyFile(string source, string dest)
        {
            using (FileStream sourceStream = new FileStream(source, FileMode.Open))
            {
                byte[] buffer = new byte[64 * 1024]; // Change to suitable size after testing performance
                using (FileStream destStream = new FileStream(dest, FileMode.Create))
                {
                    int i;
                    while ((i = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, i);
                    }
                }
            }
        }
        /// <summary>
        /// iterathe thrugh the folder and check file
        /// </summary>
        /// <param name="root"></param>
        /// <param name="backupRoot"></param>
        public void BackupRun(string root, string backupRoot)
        {
            Stack<string> dirs = new Stack<string>(20);

            if (!System.IO.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                if (!conf.IgnoreDir.Contains(currentDir))
                {


                    string[] subDirs;
                    try
                    {
                        subDirs = System.IO.Directory.GetDirectories(currentDir);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    string[] files = null;
                    try
                    {
                        files = System.IO.Directory.GetFiles(currentDir);
                    }

                    catch (UnauthorizedAccessException e)
                    {

                        Console.WriteLine(e.Message);
                        continue;
                    }

                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    foreach (string file in files)
                    {
                        try
                        {
                            // Perform whatever action is required in your scenario.
                            System.IO.FileInfo fi = new System.IO.FileInfo(file);

                            if (conf.Extensions.Contains(fi.Extension.ToLower()))
                            {
                                FileFound += 1;
                                var filePath = Path.Combine(fi.DirectoryName, fi.Name);
                                var varBackupPath = filePath.Replace(root, backupRoot);
                                if (!Compare(filePath, varBackupPath, fi.Name))
                                {
                                    Console.WriteLine("copy: " + fi.Name);
                                    FileCopied += 1;
                                    CopyFile(filePath, varBackupPath);
                                    //System.IO.File.Copy(filePath, varBackupPath, true);
                                }
                            }
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            Console.WriteLine(e.Message);
                            continue;
                        }
                    }
                    // Push the subdirectories onto the stack for traversal.
                    // This could also be done before handing the files.
                    foreach (string str in subDirs)
                        dirs.Push(str);
                }
            }
        }
    }
}