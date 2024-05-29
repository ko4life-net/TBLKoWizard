using System;
using System.IO;
using KoTblDbImporter.Models;

namespace KoTblDbImporter.Utlis
{
    public static class ConfigurationHelper
    {
        public static void CreateDefaultConfigFile(string filePath)
        {
            var defaultSettings = new ConnectionSettings
            {
                ConnectionMethod = DatabaseType.SqlServer,
                Server = "localhost\\SQLEXPRESS",
                DbName = "kodb",
                Username = "kodb_user",
                Password = "password"
            };
            string conf = "# Configuration file for database connection settings\n"
                        + "# ConnectionMethod: Specify the database type (SqlServer or MySql).\n"
                        + "# Server: Specify the server address and instance name e.q localhost\\sqlexpres or for mysql localhost:port.\n"
                        + "# DbName: Specify the database name.\n"
                        + "# Username: Specify the username.\n"
                        + "# Password: Specify the password.\n"
                        + "\n"
                        + $"ConnectionMethod={defaultSettings.ConnectionMethod}\n"
                        + $"Server={defaultSettings.Server}\n"
                        + $"DbName={defaultSettings.DbName}\n"
                        + $"Username={defaultSettings.Username}\n"
                        + $"Password={defaultSettings.Password}";

            File.WriteAllText(filePath, conf);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Created new config file: {filePath}");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"We will now begin the configuration for connecting to the database.");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static ConnectionSettings LoadConnectionSettings(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            var settings = new ConnectionSettings();

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "ConnectionMethod":
                            settings.ConnectionMethod = (DatabaseType)Enum.Parse(typeof(DatabaseType), value);
                            break;
                        case "Server":
                            settings.Server = value;
                            break;
                        case "DbName":
                            settings.DbName = value;
                            break;
                        case "Username":
                            settings.Username = value;
                            break;
                        case "Password":
                            settings.Password = value;
                            break;
                    }
                }
            }

            return settings;
        }

        public static ConnectionSettings ReadConnectionSettingsFromConsole()
        {
            Console.Write("Enter the database type (SqlServer or MySql):  ");
            Console.WriteLine("(Default is SqlServer)");

            string userInput = Console.ReadLine();
            DatabaseType connectionMethod = DatabaseType.SqlServer;

            if (!string.IsNullOrEmpty(userInput))
            {
                try
                {
                    connectionMethod = (DatabaseType)Enum.Parse(typeof(DatabaseType), userInput, true);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid database type. Defaulting to SqlServer.");
                }
            }


            Console.Write("Enter the server address: ");
            Console.WriteLine("(Default is localhost\\sqlexpress)");
            var server = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(server))
            {
                server = "localhost\\sqlexpress"; 
            }
            Console.WriteLine();
            Console.Write("Enter the database name: ");
            Console.WriteLine("(Default is kodb)");
            var dbName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dbName))
            {
                dbName = "kodb_tbl";
            }

            var username = "";
            var password = "";

            if (!connectionMethod.Equals(DatabaseType.SqlServer))
            {
                Console.WriteLine();
                Console.Write("Enter the username: ");
                Console.WriteLine("(Default is kodb_user)");
                username = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(username))
                {
                    server = "kodb_user";
                }
                Console.WriteLine();
                Console.Write("Enter the password: ");
                Console.WriteLine("(Default is kodb_user)");
                password = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(password))
                {
                    server = "kodb_user";
                }
            }

            return new ConnectionSettings
            {
                ConnectionMethod = connectionMethod,
                Server = server,
                DbName = dbName,
                Username = username,
                Password = password
            };
        }

        public static void SaveConnectionSettingsToFile(string filePath, ConnectionSettings settings)
        {
            var conf = "# Configuration file for database connection settings\n"
                     + $"# ConnectionMethod: {settings.ConnectionMethod}\n"
                     + $"# Server: {settings.Server}\n"
                     + $"# DbName: {settings.DbName}\n"
                     + $"# Username: {settings.Username}\n"
                     + $"# Password: {settings.Password}\n"
                     + "\n"
                     + $"ConnectionMethod={settings.ConnectionMethod}\n"
                     + $"Server={settings.Server}\n"
                     + $"DbName={settings.DbName}\n"
                     + $"Username={settings.Username}\n"
                     + $"Password={settings.Password}";

            File.WriteAllText(filePath, conf);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Connection settings saved to file: {filePath}");
            Console.ResetColor();
        }
    }
}