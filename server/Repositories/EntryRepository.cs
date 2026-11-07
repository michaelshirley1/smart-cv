using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface IEntryRepository
{
    Task<IEnumerable<Entry>> GetByProfileId(int profileId);
    Task<IEnumerable<Entry>> GetBySectionId(int sectionId);
    Task<Entry?> GetById(int id);
    Task<int?> GetProfileId(int id);
    Task<int> Create(Entry entry);
    Task<int> Update(Entry entry);
    Task<int> Delete(int id);
    Task<int> Reorder(IReadOnlyList<int> ids);
}

public class EntryRepository : IEntryRepository
{
    private const string Columns = "e.Id, e.SectionId, e.Heading, e.Subheading, e.Location, e.DateRange, e.SortOrder";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<EntryRepository> _logger;

    public EntryRepository(IDbConnectionFactory connectionFactory, ILogger<EntryRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Entry>> GetByProfileId(int profileId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Entries e
                INNER JOIN Sections s ON s.Id = e.SectionId
                WHERE s.ProfileId = @ProfileId
                ORDER BY e.SortOrder, e.Id
                """;
            await using var connection = await _connectionFactory.Create();
            var entries = (await connection.QueryAsync<Entry>(sql, new { ProfileId = profileId })).ToList();
            return entries;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load entries for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load entries.", ex);
        }
    }

    public async Task<IEnumerable<Entry>> GetBySectionId(int sectionId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Entries e
                WHERE e.SectionId = @SectionId
                ORDER BY e.SortOrder, e.Id
                """;
            await using var connection = await _connectionFactory.Create();
            var entries = (await connection.QueryAsync<Entry>(sql, new { SectionId = sectionId })).ToList();
            return entries;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load entries for section {SectionId}", sectionId);
            throw new RepositoryException("Failed to load entries.", ex);
        }
    }

    public async Task<Entry?> GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Entries e
                WHERE e.Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var entry = await connection.QuerySingleOrDefaultAsync<Entry>(sql, new { Id = id });
            return entry;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load entry {EntryId}", id);
            throw new RepositoryException("Failed to load the entry.", ex);
        }
    }

    public async Task<int?> GetProfileId(int id)
    {
        try
        {
            const string sql = """
                SELECT s.ProfileId
                FROM Entries e
                INNER JOIN Sections s ON s.Id = e.SectionId
                WHERE e.Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var profileId = await connection.QuerySingleOrDefaultAsync<int?>(sql, new { Id = id });
            return profileId;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to find the profile for entry {EntryId}", id);
            throw new RepositoryException("Failed to load the entry.", ex);
        }
    }

    public async Task<int> Create(Entry entry)
    {
        try
        {
            const string sql = """
                INSERT INTO Entries (SectionId, Heading, Subheading, Location, DateRange, SortOrder)
                VALUES (
                    @SectionId,
                    @Heading,
                    @Subheading,
                    @Location,
                    @DateRange,
                    (SELECT COALESCE(MAX(SortOrder) + 1, 0) FROM Entries WHERE SectionId = @SectionId))
                RETURNING Id
                """;
            await using var connection = await _connectionFactory.Create();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                entry.SectionId,
                entry.Heading,
                entry.Subheading,
                entry.Location,
                entry.DateRange,
            });
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create entry for section {SectionId}", entry.SectionId);
            throw new RepositoryException("Failed to create the entry.", ex);
        }
    }

    public async Task<int> Update(Entry entry)
    {
        try
        {
            const string sql = """
                UPDATE Entries
                SET Heading = @Heading,
                    Subheading = @Subheading,
                    Location = @Location,
                    DateRange = @DateRange
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new
            {
                entry.Id,
                entry.Heading,
                entry.Subheading,
                entry.Location,
                entry.DateRange,
            });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update entry {EntryId}", entry.Id);
            throw new RepositoryException("Failed to update the entry.", ex);
        }
    }

    public async Task<int> Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Entries
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete entry {EntryId}", id);
            throw new RepositoryException("Failed to delete the entry.", ex);
        }
    }

    public async Task<int> Reorder(IReadOnlyList<int> ids)
    {
        try
        {
            const string sql = """
                UPDATE Entries
                SET SortOrder = @SortOrder
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            await using var transaction = await connection.BeginTransactionAsync();
            var affected = await connection.ExecuteAsync(sql, ids.Select((id, index) => new { Id = id, SortOrder = index }), transaction);
            await transaction.CommitAsync();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to reorder entries");
            throw new RepositoryException("Failed to reorder entries.", ex);
        }
    }
}
