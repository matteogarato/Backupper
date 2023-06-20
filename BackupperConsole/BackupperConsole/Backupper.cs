using System.Security.Cryptography;
using System.Text;

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
        private readonly bool silentOp;

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

        /// <summary>
        ///  ignored dirs
        /// </summary>
        private HashSet<string> ignoredDirs;

        /// <summary>
        /// write buffer dimension
        /// </summary>
        private const int BufferSize = 64 * 1024;

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
            var initMsg = new StringBuilder("================== Welcome In The ==================\n" +
                               "================== Backup Utility ==================\n" +
                               "reading configuration...");
            WriteLineSilently(initMsg.ToString());
            conf = Configuration.Read();
            if (conf == null)
            {
                Configuration.CreateDefaultConfig();
                WriteLineSilently("configuration not found, created standard in Application run folder...\n" +
                                  "============ Press Any key To Continue =============");
                ReadLineSilently();
                return;
            }
            WriteLineSilently("configuration read...\nstarting...");
            try
            {
                BackupRun(conf.FromDir, conf.BackupDir);
                WriteLineSilently($"ended copy {FileCopied} of {FileFound} file");
            }
            catch (FileNotFoundException ex)
            {
                WriteLineSilently("backup source not found");
#if DEBUG
                WriteLineSilently(ex.Message);
#endif
            }
            WriteLineSilently("==================== End Backup ====================\n" +
                              "============ Press Any key To Continue =============");
            ReadLineSilently();
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
            using var destStream = new FileStream(dest, FileMode.Create);
            var buffer = new byte[BufferSize];
            int bytesRead;
            while ((bytesRead = sourceStream.Read(buffer, 0, BufferSize)) > 0)
            {
                destStream.Write(buffer, 0, bytesRead);
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
            ignoredDirs = new HashSet<string>(conf.IgnoreDir, StringComparer.InvariantCultureIgnoreCase);
            if (!Directory.Exists(root))
            {
                throw new FileNotFoundException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                var currentDir = dirs.Pop();
                if (ignoredDirs.Contains(currentDir))
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
                            WriteLineSilently(ex.Message);
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
                            WriteLineSilently(ex.Message);
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
                                    WriteLineSilently("copy: " + fi.Name);
#endif
                                    FileCopied += 1;
                                    CopyFile(filePath, backupFilePath);
                                }
                            }
                        }
                        catch (FileNotFoundException e)
                        {
                            WriteLineSilently(e.Message);
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

        private void WriteLineSilently(string msg)
        {
            if (!silentOp)
            {
                Console.WriteLine(msg);
            }
        }

        private void ReadLineSilently()
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