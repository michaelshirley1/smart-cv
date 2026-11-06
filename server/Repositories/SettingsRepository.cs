using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ISettingsRepository
{
    IEnumerable<Setting> GetAll();
    int Save(IEnumerable<Setting> settings);
}

public class SettingsRepository : ISettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(IDbConnectionFactory connectionFactory, ILogger<SettingsRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IEnumerable<Setting> GetAll()
    {
        try
        {
            const string sql = """
                SELECT Key, Value
                FROM Settings
                """;
            using var connection = _connectionFactory.Create();
            var settings = connection.Query<Setting>(sql).ToList();
            return settings;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            throw new RepositoryException("Failed to load settings.", ex);
        }
    }

    public int Save(IEnumerable<Setting> settings)
    {
        try
        {
            const string sql = """
                INSERT INTO Settings (Key, Value)
                VALUES (@Key, @Value)
                ON CONFLICT (Key) DO UPDATE SET Value = excluded.Value
                """;
            using var connection = _connectionFactory.Create();
            using var transaction = connection.BeginTransaction();
            var affected = connection.Execute(sql, settings, transaction);
            transaction.Commit();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            throw new RepositoryException("Failed to save settings.", ex);
        }
    }
}
