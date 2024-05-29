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

        public DataImporter(int clientVersion, IDatabaseConnection connection)
        {
            _clientVersion = clientVersion;
            _connection = connection;
        }

        public void ImportDataFromDirectory()
        {
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Please enter the client data location, e.g., C:\\KnightOnline\\Data");
                Console.ResetColor();
                string clientLocation = Console.ReadLine();

                if (FileHelper.DirectoryExists(clientLocation))
                {
                    var tblDatabase = LoadEncryptedData(clientLocation);

                    if (tblDatabase != null)
                    {
                        ImportData(tblDatabase);
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
                Console.ResetColor();
                return null;
            }

            DataSet tblDatabase = new DataSet();
            IEncryption encryption = EncryptionFactory.CreateEncryptor(_clientVersion);
            ProgressBar progressBar = new ProgressBar(0, tblFiles.Count(), additionalInfo: "Processing... ");

            int i = 1;

            foreach (string tblFile in tblFiles)
            {
                Console.WriteLine($"File: {tblFile}");
                byte[] fileData = encryption.ProcessFile(tblFile);

                encryption.LoadByteDataIntoDataSet(fileData, Path.GetFileNameWithoutExtension(tblFile), tblDatabase);

                progressBar.Update(i);

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"File {tblFile} successfully read.");
                Console.ResetColor();
            }

            return tblDatabase;
        }

        private void ImportData(DataSet tblDatabase)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Creating tables for tbl files and importing data...");
            Console.ResetColor();

            int tableCount = tblDatabase.Tables.Count;
            int tableIndex = 1;

            foreach (DataTable table in tblDatabase.Tables)
            {
                CreateTableAndImportData(table, tableIndex, tableCount);
                tableIndex++;
            }
        }

        private void CreateTableAndImportData(DataTable table, int tableIndex, int tableCount)
        {
            ProgressBar progressBarTables = new ProgressBar(0, tableCount, additionalInfo: "Processing... ");
            Console.WriteLine($"Table Name: {table.TableName}");

            var createTableSql = _connection.GenerateCreateTableQuery(table, table.TableName);
            _connection.ExecuteQuery(createTableSql, $"Table {table.TableName} {tableIndex} of {tableCount} created successfully");

            progressBarTables.Update(tableIndex);

            int rowIndex = 1;
            ProgressBar progressBar = new ProgressBar(0, table.Rows.Count, additionalInfo: "Processing... ");
            foreach (DataRow row in table.Rows)
            {
                var insertQuery = _connection.GenerateInsertQuery(table.TableName, row);
                _connection.ExecuteQuery(insertQuery, $"Data row {rowIndex} out of {table.Rows.Count} imported successfully");
                progressBar.Update(rowIndex);
                rowIndex++;
            }
        }
    }
}
