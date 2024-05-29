using System;
using System.Data;


namespace KoTblDbImporter.DataAccess.Connections
{
    public interface IDatabaseConnection
    {
        bool Connect(string server = "", string dbName = "", string username = null, string password = null);
        void Disconnect();
        void ExecuteQuery(string query, string comment, ConsoleColor color = ConsoleColor.Green);
        bool CreateDatabase(string databaseName);
        bool DatabaseExists(string databaseName);
        bool DropAllTables(string databaseName);
        bool CreateVersionTable();
        bool CreateVersionEntry(int clientVersion);
        DataTable GetVersionEntry();

    }
}
