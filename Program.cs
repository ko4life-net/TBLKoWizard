using System;
using System.IO;
using KoTblDbImporter.Utlis;

namespace KoTblDbImporter
{
    class Program
    {
        static void Main(string[] args) {
            try
            {
                string configFile = "config.conf";
                if (!File.Exists(configFile))
                {
                    ConfigurationHelper.CreateDefaultConfigFile(configFile);
                    var userSettings = ConfigurationHelper.ReadConnectionSettingsFromConsole();

                    ConfigurationHelper.SaveConnectionSettingsToFile(configFile, userSettings);
                }

                var settings = ConfigurationHelper.LoadConnectionSettings(configFile);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
            }

        }
    }
}
