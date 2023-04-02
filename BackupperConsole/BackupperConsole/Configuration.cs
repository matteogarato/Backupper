using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackupperConsole
{
    /// <summary>
    /// specific configuration class
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// path where replicate the backup structure
        /// </summary>
        public string BackupDir { get; set; }

        /// <summary>
        /// whitelist extension of file to backup
        /// </summary>
        public List<string> Extensions { get; set; }

        /// <summary>
        /// root of the backup
        /// </summary>
        public string FromDir { get; set; }

        /// <summary>
        /// directory to ignore (will ignore subfolder)
        /// </summary>
        public List<string> IgnoreDir { get; set; }

        /// <summary>
        /// path of the ini file
        /// </summary>
        [JsonIgnore]
        public static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config.ini");

        public Configuration()
        {
            Extensions = new List<string>();
            IgnoreDir = new List<string>();
        }
        public static void CreateDefaultConfig()
        {
            if (!ExistConfig())
            {
                var defaultConfig = new Configuration
                {
                    FromDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                    BackupDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Backup"),
                    Extensions = new List<string>
                    {
                        ".jpeg",
                        ".jpg"
                    },
                    IgnoreDir = new List<string> { "some", "directory" }
                };
                Stream fileStream = new FileStream(Path, FileMode.Create);
                using StreamWriter writer = new(fileStream);
                writer.Write(JsonSerializer.Serialize(defaultConfig));
            }
        }

        /// <summary>
        /// check the existence of the ini file
        /// </summary>
        /// <returns>true if exist</returns>
        public static bool ExistConfig()
        {
            return File.Exists(Path);
        }

        /// <summary>
        /// read the configuration file, if not exist one default will be created
        /// </summary>
        public static Configuration Read()
        {
            if (ExistConfig())
            {
                var fileStream = new FileStream(Path, FileMode.Open);
                var readed = "";
                using (StreamReader reader = new(fileStream))
                {
                    readed = reader.ReadToEnd();
                    readed = readed.Replace('\n', ' ');
                    readed = readed.Replace('\r', ' ');
                    readed = readed.Trim();
                }
                return !string.IsNullOrEmpty(readed.Trim()) ? JsonSerializer.Deserialize<Configuration>(readed) : null;
            }
            return null;
        }
    }
}