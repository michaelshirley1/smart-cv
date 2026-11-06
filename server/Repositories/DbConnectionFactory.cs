using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Host;

namespace SmartCv.Repositories;

public interface IDbConnectionFactory
{
    SqliteConnection Create();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    static DbConnectionFactory()
    {
        SqlMapper.AddTypeHandler(new JsonListTypeHandler());
    }

    public DbConnectionFactory(IAppPaths paths)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = paths.Database,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public SqliteConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
