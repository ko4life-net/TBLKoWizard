using System;

namespace TBLKoWizard.Models
{
    public enum DatabaseType
    {
        SqlServer,
        MySql
    }

    public class ConnectionSettings
    {
        public DatabaseType ConnectionMethod { get; set; }
        public string Server { get; set; }
        public string DbName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}