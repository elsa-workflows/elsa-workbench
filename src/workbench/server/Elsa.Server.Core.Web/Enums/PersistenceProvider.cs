namespace Elsa.Server.Core.Web;

public enum PersistenceProvider
{
    Memory,
    EntityFrameworkCore,
    MongoDb,
    Dapper
}