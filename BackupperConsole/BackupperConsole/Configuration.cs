using System;
using System.Collections.Generic;
using System.IO;

namespace BackupperConsole
{
    /// <summary>
    /// specific configuration class
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// root of the backup
        /// </summary>
        public string FromDir { get; set; }
        /// <summary>
        /// path where replicate the backup structure
        /// </summary>
        public string BackupDir { get; set; }
        /// <summary>
        /// directory to ignore (will ignore subfolder)
        /// </summary>
        public List<string> IgnoreDir { get; set; }
        /// <summary>
        /// whitelist extension of file to backup
        /// </summary>
        public List<string> Extensions { get; set; }
        /// <summary>
        /// path of the ini file
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// constructor
        /// </summary>
        public Configuration()
        {
            //var enviromental = Environment.GetEnvironmentVariable(IsLinux ? "Home" : "LocalAppData");
            //enviromental = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config.ini");
        }
        /// <summary>
        /// detrminates the o.s. linux=true windows=false
        /// </summary>
        /// <returns>true if linux false if windows</returns>
        public bool IsLinux()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
        /// <summary>
        /// check the existence of the ini file
        /// </summary>
        /// <returns>true if exist</returns>
        public bool ExistConfig()
        {
            bool exist = File.Exists(Path);
            return exist;
        }
        /// <summary>
        /// read the configuration file, if not exist one default will be created
        /// </summary>
        public void ReadConfigurationSafe()
        {
            if (ExistConfig())
            {
                ReadConfig();
            }
            else
            {
                Stream fileStream = new FileStream(Path, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    var from = string.Format("FromDir ={0};", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                    var backup = string.Format("BackupDir={0};", System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Backup"));
					var extension = "Extensions =.jpg &.jpeg;";
					var ignoredir = "IgnoreDir =some & directory;";
                    writer.Write(string.Format("{0}{1}{2}{3}", from, backup, extension, ignoredir));
                }
                ReadConfig();
            }
        }
        /// <summary>
        /// read the configuration from file
        /// </summary>
        private void ReadConfig()
        {
            FileStream fileStream = new FileStream(Path, FileMode.Open);
            string readed = "";
            using (StreamReader reader = new StreamReader(fileStream))
            {
                readed = reader.ReadToEnd();
                readed = readed.Replace('\n', ' ');
                readed = readed.Replace('\r', ' ');
                readed = readed.Trim();
            }
            if (!string.IsNullOrEmpty(readed.Trim()))
            {
                var configMap = readed.Split(';');
                foreach (var config in configMap)
                {
                    var Params = config.Split('=');
                    if (Params.Length == 2)
                    {
                        switch (Params[0].Trim().ToLower())
                        {
                            case "fromdir":
                                FromDir = Params[1];
                                break;
                            case "backupdir":
                                BackupDir = Params[1];
                                break;
                            case "extensions":
                                var extension = Params[1].Split('&');
                                Extensions = new List<string>();
                                foreach (string s in extension)
                                {
                                    if (!Extensions.Contains(s))
                                    {
                                        Extensions.Add(s.ToLower());
                                    }
                                }
                                break;
                            case "ignoredir":
                                var ignoredir = Params[1].Split('&');
                                IgnoreDir = new List<string>();
                                foreach (string s in ignoredir)
                                {
                                    if (!IgnoreDir.Contains(s))
                                    {
                                        IgnoreDir.Add(s.ToLower());
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
