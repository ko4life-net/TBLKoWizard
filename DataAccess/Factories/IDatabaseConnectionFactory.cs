using KoTblDbImporter.Models;
using KoTblDbImporter.DataAccess.Connections;

namespace KoTblDbImporter.DataAccess.Factories
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection GetDatabaseConnection(DatabaseType type);
    }
}
