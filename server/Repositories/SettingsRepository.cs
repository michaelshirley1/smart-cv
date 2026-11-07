using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ISettingsRepository
{
    Task<IEnumerable<Setting>> GetAll();
    Task<int> Save(IEnumerable<Setting> settings);
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

    public async Task<IEnumerable<Setting>> GetAll()
    {
        try
        {
            const string sql = """
                SELECT Key, Value
                FROM Settings
                """;
            await using var connection = await _connectionFactory.Create();
            var settings = (await connection.QueryAsync<Setting>(sql)).ToList();
            return settings;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            throw new RepositoryException("Failed to load settings.", ex);
        }
    }

    public async Task<int> Save(IEnumerable<Setting> settings)
    {
        try
        {
            const string sql = """
                INSERT INTO Settings (Key, Value)
                VALUES (@Key, @Value)
                ON CONFLICT (Key) DO UPDATE SET Value = excluded.Value
                """;
            await using var connection = await _connectionFactory.Create();
            await using var transaction = await connection.BeginTransactionAsync();
            var affected = await connection.ExecuteAsync(sql, settings, transaction);
            await transaction.CommitAsync();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            throw new RepositoryException("Failed to save settings.", ex);
        }
    }
}
