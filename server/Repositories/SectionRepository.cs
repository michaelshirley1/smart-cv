using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ISectionRepository
{
    IEnumerable<Section> GetByProfileId(int profileId);
    Section? GetById(int id);
    int Create(Section section);
    int Update(Section section);
    int Delete(int id);
    int Reorder(IReadOnlyList<int> ids);
    IEnumerable<TagLink> GetTagLinks(int profileId);
    int SetTags(int sectionId, IEnumerable<int> tagIds);
}

public class SectionRepository : ISectionRepository
{
    private const string Columns = "Id, ProfileId, Title, Kind, SortOrder, MaxLines, IsHidden, Priority";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SectionRepository> _logger;

    public SectionRepository(IDbConnectionFactory connectionFactory, ILogger<SectionRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IEnumerable<Section> GetByProfileId(int profileId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Sections
                WHERE ProfileId = @ProfileId
                ORDER BY SortOrder, Id
                """;
            using var connection = _connectionFactory.Create();
            var sections = connection.Query<Section>(sql, new { ProfileId = profileId }).ToList();
            return sections;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load sections for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load sections.", ex);
        }
    }

    public Section? GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Sections
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var section = connection.QuerySingleOrDefault<Section>(sql, new { Id = id });
            return section;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load section {SectionId}", id);
            throw new RepositoryException("Failed to load the section.", ex);
        }
    }

    public int Create(Section section)
    {
        try
        {
            const string sql = """
                INSERT INTO Sections (ProfileId, Title, Kind, SortOrder, MaxLines, IsHidden, Priority)
                VALUES (
                    @ProfileId,
                    @Title,
                    @Kind,
                    (SELECT COALESCE(MAX(SortOrder) + 1, 0) FROM Sections WHERE ProfileId = @ProfileId),
                    @MaxLines,
                    @IsHidden,
                    @Priority)
                RETURNING Id
                """;
            using var connection = _connectionFactory.Create();
            var id = connection.ExecuteScalar<int>(sql, new
            {
                section.ProfileId,
                section.Title,
                Kind = section.Kind.ToString().ToLowerInvariant(),
                section.MaxLines,
                section.IsHidden,
                section.Priority,
            });
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create section for profile {ProfileId}", section.ProfileId);
            throw new RepositoryException("Failed to create the section.", ex);
        }
    }

    public int Update(Section section)
    {
        try
        {
            const string sql = """
                UPDATE Sections
                SET Title = @Title,
                    Kind = @Kind,
                    MaxLines = @MaxLines,
                    IsHidden = @IsHidden,
                    Priority = @Priority
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new
            {
                section.Id,
                section.Title,
                Kind = section.Kind.ToString().ToLowerInvariant(),
                section.MaxLines,
                section.IsHidden,
                section.Priority,
            });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update section {SectionId}", section.Id);
            throw new RepositoryException("Failed to update the section.", ex);
        }
    }

    public int Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Sections
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete section {SectionId}", id);
            throw new RepositoryException("Failed to delete the section.", ex);
        }
    }

    public int Reorder(IReadOnlyList<int> ids)
    {
        try
        {
            const string sql = """
                UPDATE Sections
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
            _logger.LogError(ex, "Failed to reorder sections");
            throw new RepositoryException("Failed to reorder sections.", ex);
        }
    }

    public IEnumerable<TagLink> GetTagLinks(int profileId)
    {
        try
        {
            const string sql = """
                SELECT st.SectionId AS OwnerId, st.TagId
                FROM SectionTags st
                INNER JOIN Sections s ON s.Id = st.SectionId
                WHERE s.ProfileId = @ProfileId
                """;
            using var connection = _connectionFactory.Create();
            var links = connection.Query<TagLink>(sql, new { ProfileId = profileId }).ToList();
            return links;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load section tags for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load section tags.", ex);
        }
    }

    public int SetTags(int sectionId, IEnumerable<int> tagIds)
    {
        try
        {
            const string deleteSql = """
                DELETE FROM SectionTags
                WHERE SectionId = @SectionId
                """;
            const string insertSql = """
                INSERT INTO SectionTags (SectionId, TagId)
                VALUES (@SectionId, @TagId)
                """;
            using var connection = _connectionFactory.Create();
            using var transaction = connection.BeginTransaction();
            connection.Execute(deleteSql, new { SectionId = sectionId }, transaction);
            var affected = connection.Execute(insertSql, tagIds.Distinct().Select(tagId => new { SectionId = sectionId, TagId = tagId }), transaction);
            transaction.Commit();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to set tags for section {SectionId}", sectionId);
            throw new RepositoryException("Failed to save section tags.", ex);
        }
    }
}
