using System.Reflection;
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
        private static readonly string ConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.ini");

        /// <summary>
        /// Create default configuration file
        /// </summary>
        public static void CreateDefaultConfig()
        {
            var defaultConfig = new Configuration
            {
                FromDir = "From Directory",
                BackupDir = "To Directory",
                Extensions = [".jpeg", ".jpg"],
                IgnoreDir = ["some", "subfolder", "directory"]
            };
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(defaultConfig));
        }

        /// <summary>
        /// Read the configuration file
        /// </summary>
        public static Configuration Read()
        {
            if (File.Exists(ConfigFilePath))
            {
                var formattedConfig = File.ReadAllText(ConfigFilePath).Replace('\n', ' ').Replace('\r', ' ').Replace(" ", string.Empty);
                return !string.IsNullOrEmpty(formattedConfig) ? JsonSerializer.Deserialize<Configuration>(formattedConfig) : null;
            }
            return null;
        }
    }
}