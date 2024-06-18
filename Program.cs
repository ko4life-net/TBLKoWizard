using System;
using System.IO;
using TBLKoWizard.DataAccess.Connections;
using TBLKoWizard.DataAccess.Factories;
using TBLKoWizard.Utlis;

namespace TBLKoWizard
{
    class Program
    {
        static void Main(string[] args) {

            DateTime currentTime = DateTime.Now;
            string logFileName = $"{currentTime:yyyy-MM-dd_HH-mm-ss}.log";
            EventLogger _logger = new EventLogger("logs", logFileName);
            _logger.LogEvent($"Application started: {currentTime:yyyy-MM-dd_HH-mm-ss}", LogLevel.Info);

            try
            {
                string configFile = "config.conf";
                if (!File.Exists(configFile))
                {
                    _logger.LogEvent("Config file does not exist. Creating new one.", LogLevel.Info);

                    ConfigurationHelper.CreateDefaultConfigFile(configFile);
                    var userSettings = ConfigurationHelper.ReadConnectionSettingsFromConsole();
                    ConfigurationHelper.SaveConnectionSettingsToFile(configFile, userSettings);

                    _logger.LogEvent("Settings saved to config file.", LogLevel.Info);
                }

                var settings = ConfigurationHelper.LoadConnectionSettings(configFile);

                IDatabaseConnectionFactory connectionFactory = new DatabaseConnectionFactory(_logger);
                IDatabaseConnection databaseConnection = connectionFactory.GetDatabaseConnection(settings.ConnectionMethod);
                databaseConnection.Connect(settings.Server, settings.DbName, settings.Username, settings.Password);

                if (!databaseConnection.DatabaseExists(settings.DbName))
                {
                    _logger.LogEvent("Database does not exist. Creating new one.", LogLevel.Info);
                    databaseConnection.CreateDatabase(settings.DbName);
                }
                else
                {
                    Console.WriteLine();

                    if (!databaseConnection.TableVersionExists())
                    {
                        _logger.LogEvent("'_VERSION' table does not exist. Creating new one.", LogLevel.Info);
                        databaseConnection.CreateVersionTable();
                    }

                    var versionInfo = databaseConnection.GetVersionEntry();
                    string? questionAction = null;

                    if (versionInfo != null && versionInfo.Rows.Count > 0)
                    {

                        // Menu import, export
                        while (true)
                        {
                            Console.WriteLine("What would you like to do? Type 'import' or 'i' for Import and 'export' or 'e' for Export.");
                            questionAction = Console.ReadLine();
                            _logger.LogEvent("What would you like to do? Type 'import' or 'i' for Import and 'export' or 'e' for Export.", LogLevel.Info);

                            if (questionAction != null && (questionAction.ToLower() == "import" || questionAction.ToLower() == "i" || questionAction.ToLower() == "export" || questionAction.ToLower() == "e"))
                            {
                                break;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Invalid response. Please type 'import' or 'i' for Import and 'export' or 'e' for Export.");
                                Console.ResetColor();
                            }

                        }

                        if(questionAction != null && questionAction.ToLower() == "import" || questionAction.ToLower() == "i") { 

                            int versionID = (int)versionInfo.Rows[0]["VersionID"];
                            DateTime createdAt = (DateTime)versionInfo.Rows[0]["CreatedAt"];

                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Data entries from tbl files already exist in the database, for version {versionID}, created on {createdAt}.");
                            Console.ResetColor();
                            _logger.LogEvent($"Data entries from tbl files already exist in the database, for version {versionID}, created on {createdAt}.", LogLevel.Warning);

                            while (true)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Are you sure you want to delete them? Please confirm by typing 'yes' ('y') or 'no' ('n').");
                                Console.ResetColor();
                                string? question = Console.ReadLine();

                                if (question != null && (question.ToLower() == "yes" || question.ToLower() == "y" || question.ToLower() == "no" || question.ToLower() == "n"))
                                {
                                    if (question.ToLower() == "yes" || question.ToLower() == "y")
                                    {
                                        Console.WriteLine();
                                        databaseConnection.DropAllTables();
                                    }
                                    break;
                                }
                                else
                                {
                                    Console.BackgroundColor = ConsoleColor.Red;
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("Invalid response. Please type 'yes' ('y') or 'no' ('n').");
                                    Console.ResetColor();
                                }
                            }
                        }
                    }

                    //Enter Client TBL Version
                    int clientVersion;
                    while (true)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        if (questionAction != null && questionAction.ToLower() == "import" || questionAction.ToLower() == "i")
                        {
                            Console.WriteLine("Please enter the client version number from which the tbl files originate e.g. 1298");
                        } else
                        {
                            Console.WriteLine("Please enter the client version number to which the tables from the database to .tbl files are to be exported");
                        }
                        Console.ResetColor();
                        string? input = Console.ReadLine();

                        if (int.TryParse(input, out clientVersion) && clientVersion > 0)
                        {
                            if (questionAction != null && questionAction.ToLower() == "import" || questionAction.ToLower() == "i")
                            {
                                if (!databaseConnection.TableVersionExists())
                                {
                                    databaseConnection.CreateVersionTable();
                                }

                                databaseConnection.CreateVersionEntry(clientVersion);

                                //Enter Client Data Path and LoadData
                                DataImporter dataImporter = new DataImporter(clientVersion, databaseConnection, _logger);
                                dataImporter.ImportDataFromDirectory();
                            }
                            else if (questionAction != null && questionAction.ToLower() == "export" || questionAction.ToLower() == "e")
                            {
                                DataExporter dataEmporter = new DataExporter(clientVersion, databaseConnection, _logger);
                                dataEmporter.ExportDataFromDatabase();

                            }



                                databaseConnection.Disconnect();

                            break;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Invalid input. Please enter a valid version.");
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (System.NotImplementedException ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error connecting to the database: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Error connecting to the database: {ex.Message}", LogLevel.Error);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error: {ex.Message}", LogLevel.Error);
            }

        }
    }
}
