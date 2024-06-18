using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TBLKoWizard.DataAccess.Connections;
using TBLKoWizard.DataAccess.Connections.ODBC;
using TBLKoWizard.Models;
using TBLKoWizard.Utlis;

namespace TBLKoWizard.DataAccess.Factories
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly EventLogger _logger;
        public DatabaseConnectionFactory(EventLogger logger)
        {
            _logger = logger;
        }
        public IDatabaseConnection GetDatabaseConnection(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.SqlServer:
                     return new OdbcDatabaseConnection(_logger);
                case DatabaseType.MySql:
                    // return new MySQLDatabaseConnection(); 
                    throw new NotImplementedException("MySQL database connection is not implemented yet.");
                default:
                    throw new ArgumentException("Invalid database type.");
            }
        }

    }
}
