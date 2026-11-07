using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ILineRepository
{
    Task<IEnumerable<Line>> GetByProfileId(int profileId);
    Task<IEnumerable<Line>> GetByEntryId(int entryId);
    Task<Line?> GetById(int id);
    Task<int?> GetProfileId(int id);
    Task<int> Create(Line line);
    Task<int> Update(Line line);
    Task<int> Delete(int id);
    Task<int> Reorder(IReadOnlyList<int> ids);
    Task<IEnumerable<TagLink>> GetTagLinks(int profileId);
    Task<int> SetTags(int lineId, IEnumerable<int> tagIds);
}

public class LineRepository : ILineRepository
{
    private const string Columns = "l.Id, l.EntryId, l.Text, l.SortOrder, l.Override, l.Priority";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<LineRepository> _logger;

    public LineRepository(IDbConnectionFactory connectionFactory, ILogger<LineRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Line>> GetByProfileId(int profileId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Lines l
                INNER JOIN Entries e ON e.Id = l.EntryId
                INNER JOIN Sections s ON s.Id = e.SectionId
                WHERE s.ProfileId = @ProfileId
                ORDER BY l.SortOrder, l.Id
                """;
            await using var connection = await _connectionFactory.Create();
            var lines = (await connection.QueryAsync<Line>(sql, new { ProfileId = profileId })).ToList();
            return lines;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load lines for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load lines.", ex);
        }
    }

    public async Task<IEnumerable<Line>> GetByEntryId(int entryId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Lines l
                WHERE l.EntryId = @EntryId
                ORDER BY l.SortOrder, l.Id
                """;
            await using var connection = await _connectionFactory.Create();
            var lines = (await connection.QueryAsync<Line>(sql, new { EntryId = entryId })).ToList();
            return lines;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load lines for entry {EntryId}", entryId);
            throw new RepositoryException("Failed to load lines.", ex);
        }
    }

    public async Task<Line?> GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Lines l
                WHERE l.Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var line = await connection.QuerySingleOrDefaultAsync<Line>(sql, new { Id = id });
            return line;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load line {LineId}", id);
            throw new RepositoryException("Failed to load the line.", ex);
        }
    }

    public async Task<int?> GetProfileId(int id)
    {
        try
        {
            const string sql = """
                SELECT s.ProfileId
                FROM Lines l
                INNER JOIN Entries e ON e.Id = l.EntryId
                INNER JOIN Sections s ON s.Id = e.SectionId
                WHERE l.Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var profileId = await connection.QuerySingleOrDefaultAsync<int?>(sql, new { Id = id });
            return profileId;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to find the profile for line {LineId}", id);
            throw new RepositoryException("Failed to load the line.", ex);
        }
    }

    public async Task<int> Create(Line line)
    {
        try
        {
            const string sql = """
                INSERT INTO Lines (EntryId, Text, SortOrder, Override, Priority)
                VALUES (
                    @EntryId,
                    @Text,
                    (SELECT COALESCE(MAX(SortOrder) + 1, 0) FROM Lines WHERE EntryId = @EntryId),
                    @Override,
                    @Priority)
                RETURNING Id
                """;
            await using var connection = await _connectionFactory.Create();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                line.EntryId,
                line.Text,
                Override = line.Override.ToString().ToLowerInvariant(),
                line.Priority,
            });
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create line for entry {EntryId}", line.EntryId);
            throw new RepositoryException("Failed to create the line.", ex);
        }
    }

    public async Task<int> Update(Line line)
    {
        try
        {
            const string sql = """
                UPDATE Lines
                SET Text = @Text,
                    Override = @Override,
                    Priority = @Priority
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new
            {
                line.Id,
                line.Text,
                Override = line.Override.ToString().ToLowerInvariant(),
                line.Priority,
            });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update line {LineId}", line.Id);
            throw new RepositoryException("Failed to update the line.", ex);
        }
    }

    public async Task<int> Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Lines
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete line {LineId}", id);
            throw new RepositoryException("Failed to delete the line.", ex);
        }
    }

    public async Task<int> Reorder(IReadOnlyList<int> ids)
    {
        try
        {
            const string sql = """
                UPDATE Lines
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
            _logger.LogError(ex, "Failed to reorder lines");
            throw new RepositoryException("Failed to reorder lines.", ex);
        }
    }

    public async Task<IEnumerable<TagLink>> GetTagLinks(int profileId)
    {
        try
        {
            const string sql = """
                SELECT lt.LineId AS OwnerId, lt.TagId
                FROM LineTags lt
                INNER JOIN Tags t ON t.Id = lt.TagId
                WHERE t.ProfileId = @ProfileId
                """;
            await using var connection = await _connectionFactory.Create();
            var links = (await connection.QueryAsync<TagLink>(sql, new { ProfileId = profileId })).ToList();
            return links;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load line tags for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load line tags.", ex);
        }
    }

    public async Task<int> SetTags(int lineId, IEnumerable<int> tagIds)
    {
        try
        {
            const string deleteSql = """
                DELETE FROM LineTags
                WHERE LineId = @LineId
                """;
            const string insertSql = """
                INSERT INTO LineTags (LineId, TagId)
                VALUES (@LineId, @TagId)
                """;
            await using var connection = await _connectionFactory.Create();
            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(deleteSql, new { LineId = lineId }, transaction);
            var affected = await connection.ExecuteAsync(insertSql, tagIds.Distinct().Select(tagId => new { LineId = lineId, TagId = tagId }), transaction);
            await transaction.CommitAsync();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to set tags for line {LineId}", lineId);
            throw new RepositoryException("Failed to save line tags.", ex);
        }
    }
}
