using KoTblDbImporter.DataAccess.Connections;
using System;
using System.Collections.Generic;
using System.Data;
using KoTblDbImporter.Encryption;
using System.Formats.Tar;

namespace KoTblDbImporter.Utlis
{
    public class DataImporter
    {
        private readonly int _clientVersion;
        private readonly IDatabaseConnection _connection;
        private readonly EventLogger _logger;

        public DataImporter(int clientVersion, IDatabaseConnection connection, EventLogger logger)
        {
            _clientVersion = clientVersion;
            _connection = connection;
            _logger = logger;
        }

        public void ImportDataFromDirectory()
        {
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Please enter the client data location, e.g., C:\\KnightOnline\\Data");
                Console.ResetColor();
                string? clientDataLocation = Console.ReadLine();

                if (!string.IsNullOrEmpty(clientDataLocation) && FileHelper.DirectoryExists(clientDataLocation))
                {
                    _logger.LogEvent($"Data folder path set to {clientDataLocation}.", LogLevel.Info);

                    var tblDatabase = LoadEncryptedData(clientDataLocation);

                    if (tblDatabase != null)
                    {
                        ImportData(tblDatabase);
                    } else
                    {
                        _logger.LogEvent("Decrypted data was empty. Decryption error.", LogLevel.Error);
                    }

                    break;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Invalid input. Please enter a valid directory path.");
                    Console.ResetColor();
                }
            }
        }

        private DataSet LoadEncryptedData(string clientLocation)
        {
            string[] tblFiles = FileHelper.GetTblFileNames(clientLocation);

            if (tblFiles.Length == 0)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("No *.tbl files found in the specified directory.");
                Console.WriteLine("Please make sure you have set the correct folder.");
                Console.ResetColor();
                _logger.LogEvent("No *.tbl files found in the specified directory.", LogLevel.Error);
                _logger.LogEvent("Please make sure you have set the correct folder.", LogLevel.Warning);

                return new DataSet();
            }

            DataSet tblDatabase = new DataSet();
            IEncryption encryption = EncryptionFactory.CreateEncryptor(_clientVersion);
            ProgressBar progressBar = new ProgressBar(0, tblFiles.Count(), additionalInfo: "Processing... ");

            int i = 1;

            foreach (string tblFile in tblFiles)
            {
                Console.WriteLine($"File: {tblFile}");
                _logger.LogEvent($"Currently decrypting file: {tblFile}", LogLevel.Info);
                byte[] fileData = encryption.ProcessFile(tblFile);

                encryption.LoadByteDataIntoDataSet(fileData, Path.GetFileNameWithoutExtension(tblFile), tblDatabase);

                progressBar.Update(i);

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"File {tblFile} successfully read.");
                Console.ResetColor();
                _logger.LogEvent($"File {tblFile} decrypted successfully.", LogLevel.Info);
            }

            return tblDatabase;
        }

        private void ImportData(DataSet tblDatabase)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Creating tables for tbl files and importing data...");
            Console.ResetColor();
            _logger.LogEvent("Creating tables for tbl files and importing data...", LogLevel.Info);

            int tableCount = tblDatabase.Tables.Count;
            int tableIndex = 1;

            DateTime currentTime = DateTime.Now;
            string logFileName = $"Database_queries_{currentTime:yyyy-MM-dd_HH-mm-ss}.log";

            var insertQueryLog = new EventLogger("logs", logFileName);

            foreach (DataTable table in tblDatabase.Tables)
            {
                CreateTableAndImportData(table, tableIndex, tableCount, insertQueryLog);
                tableIndex++;
            }

            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.White;

            string text = "Import complete.";
            int frameWidth = text.Length + 2;

            Console.WriteLine("╔" + new string('═', frameWidth) + "╗");
            Console.WriteLine("║ " + text + " ║");
            Console.WriteLine("╚" + new string('═', frameWidth) + "╝");
            Console.ResetColor();

            _logger.LogEvent("Import complete.", LogLevel.Info);
        }

        private void CreateTableAndImportData(DataTable table, int tableIndex, int tableCount, EventLogger inserQueryLogger)
        {
            ProgressBar progressBarTables = new ProgressBar(0, tableCount, additionalInfo: "Processing... ");
            Console.WriteLine($"Table Name: {table.TableName}");

            var createTableSql = _connection.GenerateCreateTableQuery(table, table.TableName);
            _connection.ExecuteQuery(createTableSql, $"Table {table.TableName} {tableIndex} of {tableCount} created successfully");

            inserQueryLogger.LogEvent(createTableSql, LogLevel.Empty);

            progressBarTables.Update(tableIndex);

            int rowIndex = 1;
            ProgressBar progressBar = new ProgressBar(0, table.Rows.Count, additionalInfo: "Processing... ");
            foreach (DataRow row in table.Rows)
            {
                var insertQuery = _connection.GenerateInsertQuery(table.TableName, row);
                _connection.ExecuteQuery(insertQuery, $"Data row {rowIndex} out of {table.Rows.Count} imported successfully");
                inserQueryLogger.LogEvent(insertQuery, LogLevel.Empty);
                progressBar.Update(rowIndex);
                rowIndex++;
            }
        }
    }
}
