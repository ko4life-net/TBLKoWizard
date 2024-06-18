using System;
using System.Data.Common;
using System.Data;
using System.Data.Odbc;
using TBLKoWizard.DataAccess.Connections;
using TBLKoWizard.Utlis;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TBLKoWizard.DataAccess.Connections.ODBC
{
    public class OdbcDatabaseConnection : IDatabaseConnection
    {
        private OdbcConnection? _connection;
        private readonly EventLogger _logger;

        public OdbcDatabaseConnection(EventLogger logger)
        {
            this._logger = logger;
        }

        public bool Connect(string server = "", string dbName = "", string username = "", string password = "")
        {
            try
            {
                string connectionString = $"Driver={{SQL Server}};Server={(string.IsNullOrEmpty(server) ? "localhost\\sqlexpress" : server)};Database={(string.IsNullOrEmpty(dbName) ? "kodb_tbl" : dbName)};";
                _connection = new OdbcConnection(connectionString);
                _connection.Open();
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Connected to the database using SqlServer.");
                Console.ResetColor();
                _logger.LogEvent("Connected to SQL Server.", LogLevel.Info);

                return true;
            }
            catch (OdbcException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error connecting to the database: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Error connecting to the database: {ex.Message}", LogLevel.Error);

                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error:: {ex.Message}", LogLevel.Error);

                return false;
            }
        }
        public void Disconnect()
        {
            try
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Disconnected from the database.");
                    Console.ResetColor();
                    _logger.LogEvent("Disconnected from the database.", LogLevel.Info);
                }
            }
            catch (OdbcException ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error disconnecting from the database: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Error disconnecting from the database: {ex.Message}", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error: {ex.Message}", LogLevel.Error);
            }
        }

        public void ExecuteQuery(string sql, string comment, ConsoleColor color = ConsoleColor.Green)
        {
            try
            {
                if (_connection != null && _connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                using (OdbcCommand command = new OdbcCommand(sql, _connection))
                {
                    command.ExecuteNonQuery();
                    Console.ForegroundColor = color;
                    Console.WriteLine($"{comment}");
                    Console.ResetColor();
                    _logger.LogEvent($"{comment}", LogLevel.Info);
                }
            }
            catch (OdbcException ex)
            {
                if (ex.Errors.Count > 0)
                {
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (OdbcError error in ex.Errors)
                    {
                        Console.WriteLine($"SQL Error {error.SQLState}: {error.Message}");
                        _logger.LogEvent($"SQL Error {error.SQLState}: {error.Message}", LogLevel.Error);

                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ODBC Error: {ex.Message}");
                    Console.ResetColor();
                    _logger.LogEvent($"ODBC Error: {ex.Message}", LogLevel.Error);
                }
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error: {ex.Message}", LogLevel.Error);
            }
        }

        public bool CreateDatabase(string databaseName)
        {
            string sql = $"CREATE DATABASE [{databaseName}]";
            try
            {
                using (OdbcCommand command = new OdbcCommand(sql, _connection))
                {
                    command.ExecuteNonQuery();
                    _logger.LogEvent($"Database {databaseName} created successfully.", LogLevel.Info);

                    return true;
                }
            }
            catch (OdbcException ex)
            {
                string errorMessage = ex.Message;

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error creating database '{databaseName}': {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Error creating database '{databaseName}': {errorMessage}", LogLevel.Error);

                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error: {errorMessage}", LogLevel.Error);

                return false;
            }
        }

        public bool DatabaseExists(string databaseName)
        {
            string sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";

            if (_connection != null)
            {
                using (OdbcCommand command = _connection.CreateCommand())
                {
                    command.CommandText = sql;
                    int databaseCount = Convert.ToInt32(command.ExecuteScalar());
                    bool databaseExists = databaseCount > 0;

                    return databaseExists;
                }
            }

            return false;
        }

        public bool DropAllTables()
        {
            try
            {
                if (_connection != null)
                {
                    if (_connection.State != ConnectionState.Open)
                    {
                        _connection.Open();
                    }

                    DataTable tables = _connection.GetSchema("Tables");
                    ProgressBar progressBar = new ProgressBar(0, tables.Rows.Count - 2, additionalInfo: "Processing... ");
                    int i = 1;

                    foreach (DataRow row in tables.Rows)
                    {
                        string? tableName = row["TABLE_NAME"].ToString();

                        if (tableName != null) { 
                            if (tableName.StartsWith("sys") || tableName == "trace_xe_action_map" || tableName == "trace_xe_event_map")
                                continue;

                            string dropTableSql = $"DROP TABLE [{tableName}]";
                            ExecuteQuery(dropTableSql, $"Table '{tableName}' {i} of {tables.Rows.Count - 2} was dropped successfully.");

                        }
                        progressBar.Update(i);
                        i++;
                    }

                    return true;
                }

                return false;
            }
            catch (OdbcException ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error dropping tables: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Error dropping tables: {ex.Message}", LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        public bool TableVersionExists()
        {
            string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '_VERSION'";

            try
            {
                using (OdbcCommand command = new OdbcCommand(sql, _connection))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (OdbcException ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error checking if '_VERSION' table exists: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
        public bool CreateVersionTable()
        {
            string sql = "CREATE TABLE _VERSION (VersionID INT, CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP)";
            ExecuteQuery(sql, $"Successful creation of _VERSION table.");

            return true;
        }
        public bool CreateVersionEntry(int clientVersion)
        {
            string sql = $"INSERT INTO _VERSION (VersionID) VALUES ({clientVersion});";
            Console.WriteLine();
            ExecuteQuery(sql, $"The entry with client version has been created in the _VERSION table.");

            return true;
        }
        public DataTable GetVersionEntry()
        {
            if(_connection != null) { 
                string sql = "SELECT * FROM _VERSION";
                using (OdbcCommand command = _connection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }

            return new DataTable(); 
        }

        public string MapDataTypeToString(Type dataType)
        {
            Dictionary<Type, string> typeMappings = new Dictionary<Type, string>
            {
                { typeof(int), "INT" },
                { typeof(string), "VARCHAR(MAX)" },
                { typeof(double), "FLOAT" },
                { typeof(sbyte), "SMALLINT" }, //Tinyint ?
                { typeof(byte), "SMALLINT" },
                { typeof(short), "SMALLINT" },
                { typeof(uint), "INT" },
                { typeof(float), "REAL" }
            };

            if (typeMappings.ContainsKey(dataType))
            {
                return typeMappings[dataType];
            }
            else
            {
                _logger.LogEvent($"Data type {dataType} not supported.", LogLevel.Error);
                throw new NotSupportedException($"Data type {dataType} not supported.");
            }
        }

        public string GenerateCreateTableQuery(DataTable table, string tableName)
        {
            string createTableSQL = $"CREATE TABLE {tableName} (\n";

            foreach (DataColumn column in table.Columns)
            {
                string columnName = "" + column.ColumnName;
                Type dataType = column.DataType;
                string sqlType = MapDataTypeToString(dataType);

                createTableSQL += $"    {columnName} {sqlType},\n";
            }

            createTableSQL = createTableSQL.TrimEnd(',', '\n') + "\n);";

            return createTableSQL;
        }

        public string GenerateInsertQuery(string tableName, DataRow row)
        {
            StringBuilder insertQuery = new StringBuilder($"INSERT INTO {tableName} (");

            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            foreach (DataColumn column in row.Table.Columns)
            {
                columns.Add($"{column.ColumnName}");

                var item = row[column];

                string? value;
                if (item is string)
                {
                    value = $"'{((string)item).Replace("'", "''")}'";
                }
                else if (item is float || item is double || item is decimal)
                {
                    value = ((IFormattable)item).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    value = item.ToString();
                }

                values.Add(value!);
            }

            insertQuery.Append(string.Join(", ", columns));

            insertQuery.Append(") VALUES (");

            insertQuery.Append(string.Join(", ", values));

            insertQuery.Append(");");

            return insertQuery.ToString();
        }

        public DataSet GetAllTablesToDataset()
        {
            DataSet dataSet = new DataSet();

            try
            {
                if (_connection != null)
                {
                    if (_connection.State != ConnectionState.Open)
                    {
                        _connection.Open();
                    }

                    DataTable schemaTable = _connection.GetSchema("Tables");

                    var tables = from DataRow row in schemaTable.Rows
                                     where !row["TABLE_NAME"].ToString().StartsWith("sys")
                                        && !row["TABLE_NAME"].ToString().StartsWith("trace_xe_")
                                        && !row["TABLE_NAME"].ToString().StartsWith("_VERSION")
                                     select row;

                    foreach (DataRow row in tables)
                    {
                        string tableName = row["TABLE_NAME"].ToString();


                        string query = $"SELECT * FROM [{tableName}]";
                        using (OdbcCommand command = new OdbcCommand(query, _connection))
                        using (OdbcDataAdapter adapter = new OdbcDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable(tableName);
                            adapter.Fill(dataTable);
                            dataSet.Tables.Add(dataTable);
                        }
                    }
                }
                return dataSet;
            }
            catch (OdbcException ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error loading tables: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Error loading tables: {ex.Message}", LogLevel.Error);
                return dataSet;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.ResetColor();
                _logger.LogEvent($"Unexpected error: {ex.Message}", LogLevel.Error);
                return dataSet;
            }

            return dataSet;
        }
    }
}