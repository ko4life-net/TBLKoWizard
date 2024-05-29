using System;
using System.IO;
using KoTblDbImporter.DataAccess.Connections;
using KoTblDbImporter.DataAccess.Factories;
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

                IDatabaseConnectionFactory connectionFactory = new DatabaseConnectionFactory();
                IDatabaseConnection databaseConnection = connectionFactory.GetDatabaseConnection(settings.ConnectionMethod);
                databaseConnection.Connect(settings.Server, settings.DbName, settings.Username, settings.Password);


            }
            catch (System.NotImplementedException ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error connecting to the database: {ex.Message}");
                Console.ResetColor();
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
