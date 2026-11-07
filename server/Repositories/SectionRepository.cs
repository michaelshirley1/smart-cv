using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ISectionRepository
{
    Task<IEnumerable<Section>> GetByProfileId(int profileId);
    Task<Section?> GetById(int id);
    Task<int> Create(Section section);
    Task<int> Update(Section section);
    Task<int> Delete(int id);
    Task<int> Reorder(IReadOnlyList<int> ids);
    Task<IEnumerable<TagLink>> GetTagLinks(int profileId);
    Task<int> SetTags(int sectionId, IEnumerable<int> tagIds);
}

public class SectionRepository : ISectionRepository
{
    private const string Columns = "Id, ProfileId, Title, Kind, SortOrder, MaxLines, IsHidden, Priority, MaxTotal";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SectionRepository> _logger;

    public SectionRepository(IDbConnectionFactory connectionFactory, ILogger<SectionRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Section>> GetByProfileId(int profileId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Sections
                WHERE ProfileId = @ProfileId
                ORDER BY SortOrder, Id
                """;
            await using var connection = await _connectionFactory.Create();
            var sections = (await connection.QueryAsync<Section>(sql, new { ProfileId = profileId })).ToList();
            return sections;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load sections for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load sections.", ex);
        }
    }

    public async Task<Section?> GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Sections
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var section = await connection.QuerySingleOrDefaultAsync<Section>(sql, new { Id = id });
            return section;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load section {SectionId}", id);
            throw new RepositoryException("Failed to load the section.", ex);
        }
    }

    public async Task<int> Create(Section section)
    {
        try
        {
            const string sql = """
                INSERT INTO Sections (ProfileId, Title, Kind, SortOrder, MaxLines, IsHidden, Priority, MaxTotal)
                VALUES (
                    @ProfileId,
                    @Title,
                    @Kind,
                    (SELECT COALESCE(MAX(SortOrder) + 1, 0) FROM Sections WHERE ProfileId = @ProfileId),
                    @MaxLines,
                    @IsHidden,
                    @Priority,
                    @MaxTotal)
                RETURNING Id
                """;
            await using var connection = await _connectionFactory.Create();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                section.ProfileId,
                section.Title,
                Kind = section.Kind.ToString().ToLowerInvariant(),
                section.MaxLines,
                section.IsHidden,
                section.Priority,
                section.MaxTotal,
            });
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create section for profile {ProfileId}", section.ProfileId);
            throw new RepositoryException("Failed to create the section.", ex);
        }
    }

    public async Task<int> Update(Section section)
    {
        try
        {
            const string sql = """
                UPDATE Sections
                SET Title = @Title,
                    Kind = @Kind,
                    MaxLines = @MaxLines,
                    IsHidden = @IsHidden,
                    Priority = @Priority,
                    MaxTotal = @MaxTotal
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new
            {
                section.Id,
                section.Title,
                Kind = section.Kind.ToString().ToLowerInvariant(),
                section.MaxLines,
                section.IsHidden,
                section.Priority,
                section.MaxTotal,
            });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update section {SectionId}", section.Id);
            throw new RepositoryException("Failed to update the section.", ex);
        }
    }

    public async Task<int> Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Sections
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete section {SectionId}", id);
            throw new RepositoryException("Failed to delete the section.", ex);
        }
    }

    public async Task<int> Reorder(IReadOnlyList<int> ids)
    {
        try
        {
            const string sql = """
                UPDATE Sections
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
            _logger.LogError(ex, "Failed to reorder sections");
            throw new RepositoryException("Failed to reorder sections.", ex);
        }
    }

    public async Task<IEnumerable<TagLink>> GetTagLinks(int profileId)
    {
        try
        {
            const string sql = """
                SELECT st.SectionId AS OwnerId, st.TagId
                FROM SectionTags st
                INNER JOIN Sections s ON s.Id = st.SectionId
                WHERE s.ProfileId = @ProfileId
                """;
            await using var connection = await _connectionFactory.Create();
            var links = (await connection.QueryAsync<TagLink>(sql, new { ProfileId = profileId })).ToList();
            return links;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load section tags for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load section tags.", ex);
        }
    }

    public async Task<int> SetTags(int sectionId, IEnumerable<int> tagIds)
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
            await using var connection = await _connectionFactory.Create();
            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(deleteSql, new { SectionId = sectionId }, transaction);
            var affected = await connection.ExecuteAsync(insertSql, tagIds.Distinct().Select(tagId => new { SectionId = sectionId, TagId = tagId }), transaction);
            await transaction.CommitAsync();
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to set tags for section {SectionId}", sectionId);
            throw new RepositoryException("Failed to save section tags.", ex);
        }
    }
}
