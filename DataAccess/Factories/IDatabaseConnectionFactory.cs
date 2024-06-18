using TBLKoWizard.Models;
using TBLKoWizard.DataAccess.Connections;

namespace TBLKoWizard.DataAccess.Factories
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection GetDatabaseConnection(DatabaseType type);
    }
}
