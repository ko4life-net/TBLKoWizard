using TBLKoWizard.DataAccess.Connections;
using TBLKoWizard.Encryption;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBLKoWizard.Utlis
{
    internal class DataExporter
    {
        private readonly int _clientVersion;
        private readonly IDatabaseConnection _connection;
        private readonly EventLogger _logger;

        public DataExporter(int clientVersion, IDatabaseConnection connection, EventLogger logger)
        {
            _clientVersion = clientVersion;
            _connection = connection;
            _logger = logger;
        }

        public void ExportDataFromDatabase()
        {
            string? exportDataLocation = null;
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Please enter export data location, e.g., C:\\KnightOnline\\Data");
                Console.WriteLine("If you leave the field blank, files will be saved to the 'export' folder.");
                Console.ResetColor();
                exportDataLocation = Console.ReadLine();

                if (string.IsNullOrEmpty(exportDataLocation))
                {
                    exportDataLocation = "export";

                    if (!Directory.Exists(exportDataLocation))
                    {
                        Directory.CreateDirectory(exportDataLocation);
                    }
                    _logger.LogEvent($"Export folder path set to {exportDataLocation}.", LogLevel.Info);

                    break;

                }
                else if (!string.IsNullOrEmpty(exportDataLocation) && FileHelper.DirectoryExists(exportDataLocation))
                {
                    _logger.LogEvent($"Export folder path set to {exportDataLocation}.", LogLevel.Info);
                    break;
                }
            }

            DataSet tblDatabase = null;

            tblDatabase = _connection.GetAllTablesToDataset();

            if (tblDatabase != null)
            {
                SaveDataSetToTblFiles(exportDataLocation, tblDatabase);
            }
            else
            {
                _logger.LogEvent("Database is empty.", LogLevel.Error);
            }

        }

        public bool SaveDataSetToTblFiles(string directoryPath, DataSet dataSet)
        {
            try
            {

                ProgressBar progressBar = new ProgressBar(0, dataSet.Tables.Count, additionalInfo: "Processing... ");

                int i = 1;
                foreach (DataTable table in dataSet.Tables)
                {
                    string tableName = table.TableName;
                    string filePath = Path.Combine(directoryPath, $"{tableName}.tbl");

                    if (!SaveTableToFile(filePath, table))
                    {
                        _logger.LogEvent($"Error while saving table '{tableName}' to file.", LogLevel.Error);
                        return false;
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"File {i} of {dataSet.Tables.Count} {tableName}.tbl successfully exported.");
                    Console.ResetColor();
                    progressBar.Update(i);

                    i++;
                }

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;

                string text = "Data from the database has been successfully exported to .tbl files.";
                int frameWidth = text.Length + 2;

                Console.WriteLine("╔" + new string('═', frameWidth) + "╗");
                Console.WriteLine("║ " + text + " ║");
                Console.WriteLine("╚" + new string('═', frameWidth) + "╝");
                Console.ResetColor();

                _logger.LogEvent("Data from the database has been successfully exported to .tbl files.", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogEvent($"Error while saving dataset to .tbl files: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        public bool SaveTableToFile(string fname, DataTable table)
        {
            try
            {
                using (var outStream = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    _logger.LogEvent($"Saving {fname}.tbl ", LogLevel.Info);

                    outStream.Write(BitConverter.GetBytes(table.Columns.Count), 0, 4);

                    foreach (DataColumn column in table.Columns)
                    {
                        switch (Type.GetTypeCode(column.DataType))
                        {
                            case TypeCode.Single:
                                outStream.Write(BitConverter.GetBytes(8), 0, 4);
                                break;
                            case TypeCode.String:
                                outStream.Write(BitConverter.GetBytes(7), 0, 4);
                                break;
                            case TypeCode.UInt32:
                                outStream.Write(BitConverter.GetBytes(6), 0, 4);
                                break;
                            case TypeCode.Int32:
                                outStream.Write(BitConverter.GetBytes(5), 0, 4);
                                break;
                            case TypeCode.Int16:
                                outStream.Write(BitConverter.GetBytes(3), 0, 4);
                                break;
                            case TypeCode.Byte:
                                outStream.Write(BitConverter.GetBytes(2), 0, 4);
                                break;
                            case TypeCode.SByte:
                                outStream.Write(BitConverter.GetBytes(1), 0, 4);
                                break;
                            default:
                                outStream.Write(BitConverter.GetBytes(5), 0, 4);
                                break;
                        }
                    }

                    outStream.Write(BitConverter.GetBytes(table.Rows.Count), 0, 4);

                    foreach (DataRow row in table.Rows)
                    {
                        for (int column = 0; column < table.Columns.Count; column++)
                        {
                            WriteValueToStream(outStream, table.Columns[column].DataType, row[column]);
                        }
                    }

                    IEncryption encryption = EncryptionFactory.CreateEncryptor(_clientVersion);
                    outStream.Seek(0, SeekOrigin.Begin);
                    encryption.Encode(outStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogEvent($"Error saving table {table.TableName}: {ex.Message}", LogLevel.Error);
                return false;
            }

            return true;
        }


        private static void WriteValueToStream(FileStream stream, Type dataType, object value)
        {
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.Single:
                    stream.Write(BitConverter.GetBytes((float)value), 0, 4);
                    break;
                case TypeCode.String:
                    byte[] stringBytes = Encoding.UTF8.GetBytes((string)value);
                    stream.Write(BitConverter.GetBytes(stringBytes.Length), 0, 4);
                    stream.Write(stringBytes, 0, stringBytes.Length);
                    break;
                case TypeCode.UInt32:
                    stream.Write(BitConverter.GetBytes((uint)value), 0, 4);
                    break;
                case TypeCode.Int32:
                    stream.Write(BitConverter.GetBytes((int)value), 0, 4);
                    break;
                case TypeCode.Int16:
                    stream.Write(BitConverter.GetBytes((short)value), 0, 2);
                    break;
                case TypeCode.Byte:
                    stream.WriteByte((byte)value);
                    break;
                case TypeCode.SByte:
                    stream.WriteByte((byte)((sbyte)value));
                    break;
                default:
                    stream.Write(BitConverter.GetBytes(Convert.ToInt32(value)), 0, 4);
                    break;
            }
        }
    }
}
