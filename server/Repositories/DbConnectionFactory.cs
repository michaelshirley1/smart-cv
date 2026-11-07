using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Host;

namespace SmartCv.Repositories;

public interface IDbConnectionFactory
{
    Task<SqliteConnection> Create();
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

    public async Task<SqliteConnection> Create()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
