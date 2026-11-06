using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ILineRepository
{
    IEnumerable<Line> GetByProfileId(int profileId);
    IEnumerable<Line> GetByEntryId(int entryId);
    Line? GetById(int id);
    int? GetProfileId(int id);
    int Create(Line line);
    int Update(Line line);
    int Delete(int id);
    int Reorder(IReadOnlyList<int> ids);
    IEnumerable<TagLink> GetTagLinks(int profileId);
    int SetTags(int lineId, IEnumerable<int> tagIds);
}

public class LineRepository : ILineRepository
{
    private const string Columns = "l.Id, l.EntryId, l.Text, l.SortOrder, l.Override";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<LineRepository> _logger;

    public LineRepository(IDbConnectionFactory connectionFactory, ILogger<LineRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IEnumerable<Line> GetByProfileId(int profileId)
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
            using var connection = _connectionFactory.Create();
            var lines = connection.Query<Line>(sql, new { ProfileId = profileId }).ToList();
            return lines;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load lines for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load lines.", ex);
        }
    }

    public IEnumerable<Line> GetByEntryId(int entryId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Lines l
                WHERE l.EntryId = @EntryId
                ORDER BY l.SortOrder, l.Id
                """;
            using var connection = _connectionFactory.Create();
            var lines = connection.Query<Line>(sql, new { EntryId = entryId }).ToList();
            return lines;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load lines for entry {EntryId}", entryId);
            throw new RepositoryException("Failed to load lines.", ex);
        }
    }

    public Line? GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Lines l
                WHERE l.Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var line = connection.QuerySingleOrDefault<Line>(sql, new { Id = id });
            return line;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load line {LineId}", id);
            throw new RepositoryException("Failed to load the line.", ex);
        }
    }

    public int? GetProfileId(int id)
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
            using var connection = _connectionFactory.Create();
            var profileId = connection.QuerySingleOrDefault<int?>(sql, new { Id = id });
            return profileId;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to find the profile for line {LineId}", id);
            throw new RepositoryException("Failed to load the line.", ex);
        }
    }

    public int Create(Line line)
    {
        try
        {
            const string sql = """
                INSERT INTO Lines (EntryId, Text, SortOrder, Override)
                VALUES (
                    @EntryId,
                    @Text,
                    (SELECT COALESCE(MAX(SortOrder) + 1, 0) FROM Lines WHERE EntryId = @EntryId),
                    @Override)
                RETURNING Id
                """;
            using var connection = _connectionFactory.Create();
            var id = connection.ExecuteScalar<int>(sql, new
            {
                line.EntryId,
                line.Text,
                Override = line.Override.ToString().ToLowerInvariant(),
            });
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create line for entry {EntryId}", line.EntryId);
            throw new RepositoryException("Failed to create the line.", ex);
        }
    }

    public int Update(Line line)
    {
        try
        {
            const string sql = """
                UPDATE Lines
                SET Text = @Text,
                    Override = @Override
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new
            {
                line.Id,
                line.Text,
                Override = line.Override.ToString().ToLowerInvariant(),
            });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update line {LineId}", line.Id);
            throw new RepositoryException("Failed to update the line.", ex);
        }
    }

    public int Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Lines
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete line {LineId}", id);
            throw new RepositoryException("Failed to delete the line.", ex);
        }
    }

    public int Reorder(IReadOnlyList<int> ids)
    {
        try
        {
            const string sql = """
                UPDATE Lines
                SET SortOrder = @SortOrder
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            using var transaction = connection.BeginTransaction();
            var affected = connection.Execute(sql, ids.Select((id, index) => new { Id = id, SortOrder = index }), transaction);
            transaction.Commit();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to reorder lines");
            throw new RepositoryException("Failed to reorder lines.", ex);
        }
    }

    public IEnumerable<TagLink> GetTagLinks(int profileId)
    {
        try
        {
            const string sql = """
                SELECT lt.LineId AS OwnerId, lt.TagId
                FROM LineTags lt
                INNER JOIN Tags t ON t.Id = lt.TagId
                WHERE t.ProfileId = @ProfileId
                """;
            using var connection = _connectionFactory.Create();
            var links = connection.Query<TagLink>(sql, new { ProfileId = profileId }).ToList();
            return links;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load line tags for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load line tags.", ex);
        }
    }

    public int SetTags(int lineId, IEnumerable<int> tagIds)
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
            using var connection = _connectionFactory.Create();
            using var transaction = connection.BeginTransaction();
            connection.Execute(deleteSql, new { LineId = lineId }, transaction);
            var affected = connection.Execute(insertSql, tagIds.Distinct().Select(tagId => new { LineId = lineId, TagId = tagId }), transaction);
            transaction.Commit();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to set tags for line {LineId}", lineId);
            throw new RepositoryException("Failed to save line tags.", ex);
        }
    }
}
