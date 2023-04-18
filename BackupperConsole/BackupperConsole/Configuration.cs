using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// Path where replicate the backup structure
        /// </summary>
        public string BackupDir { get; set; }

        /// <summary>
        /// Whitelist extension of file to backup
        /// </summary>
        public List<string> Extensions { get; set; }

        /// <summary>
        /// Root of the backup
        /// </summary>
        public string FromDir { get; set; }

        /// <summary>
        /// Directory to ignore (will ignore subfolder)
        /// </summary>
        public List<string> IgnoreDir { get; set; }

        /// <summary>
        /// Path of the ini file
        /// </summary>
        [JsonIgnore]
        private static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config.ini");

        /// <summary>
        /// Create default configuration file
        /// </summary>
        public static void CreateDefaultConfig()
        {
            var defaultConfig = new Configuration
            {
                FromDir = "From Directory",
                BackupDir = "To Directory",
                Extensions = new List<string>
                    {
                        ".jpeg",
                        ".jpg"
                    },
                IgnoreDir = new List<string> { "some", "subfolder", "directory" }
            };
            Stream fileStream = new FileStream(Path, FileMode.OpenOrCreate);
            using StreamWriter writer = new(fileStream);
            writer.Write(JsonSerializer.Serialize(defaultConfig));
        }

        /// <summary>
        /// Read the configuration file
        /// </summary>
        public static Configuration Read()
        {
            if (File.Exists(Path))
            {
                var fileStream = new FileStream(Path, FileMode.Open);
                using (StreamReader reader = new(fileStream))
                {
                    var sb = new StringBuilder(reader.ReadToEnd());
                    sb.Replace('\n', ' ');
                    sb.Replace('\r', ' ');
                    sb.Replace(" ", string.Empty);
                    var formattedConfig = sb.ToString();
                    return !string.IsNullOrEmpty(formattedConfig) ? JsonSerializer.Deserialize<Configuration>(formattedConfig) : null;
                }
            }
            return null;
        }
    }
}