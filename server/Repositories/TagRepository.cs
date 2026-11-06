using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface ITagRepository
{
    IEnumerable<Tag> GetByProfileId(int profileId);
    Tag? GetById(int id);
    int Create(Tag tag);
    int Update(Tag tag);
    int Delete(int id);
}

public class TagRepository : ITagRepository
{
    private const string Columns = """
        t.Id, t.ProfileId, t.Name, t.Keywords, t.Colour, t.IsAlwaysOn,
        (SELECT COUNT(*) FROM LineTags lt WHERE lt.TagId = t.Id)
            + (SELECT COUNT(*) FROM SectionTags st WHERE st.TagId = t.Id) AS UsageCount
        """;

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<TagRepository> _logger;

    public TagRepository(IDbConnectionFactory connectionFactory, ILogger<TagRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IEnumerable<Tag> GetByProfileId(int profileId)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Tags t
                WHERE t.ProfileId = @ProfileId
                ORDER BY t.IsAlwaysOn DESC, t.Name COLLATE NOCASE
                """;
            using var connection = _connectionFactory.Create();
            var tags = connection.Query<Tag>(sql, new { ProfileId = profileId }).ToList();
            return tags;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load tags for profile {ProfileId}", profileId);
            throw new RepositoryException("Failed to load tags.", ex);
        }
    }

    public Tag? GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Tags t
                WHERE t.Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var tag = connection.QuerySingleOrDefault<Tag>(sql, new { Id = id });
            return tag;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load tag {TagId}", id);
            throw new RepositoryException("Failed to load the tag.", ex);
        }
    }

    public int Create(Tag tag)
    {
        try
        {
            const string sql = """
                INSERT INTO Tags (ProfileId, Name, Keywords, Colour, IsAlwaysOn)
                VALUES (@ProfileId, @Name, @Keywords, @Colour, @IsAlwaysOn)
                RETURNING Id
                """;
            using var connection = _connectionFactory.Create();
            var id = connection.ExecuteScalar<int>(sql, new
            {
                tag.ProfileId,
                tag.Name,
                tag.Keywords,
                tag.Colour,
                tag.IsAlwaysOn,
            });
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create tag {TagName} for profile {ProfileId}", tag.Name, tag.ProfileId);
            throw new RepositoryException("Failed to create the tag.", ex);
        }
    }

    public int Update(Tag tag)
    {
        try
        {
            const string sql = """
                UPDATE Tags
                SET Name = @Name,
                    Keywords = @Keywords,
                    Colour = @Colour
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new
            {
                tag.Id,
                tag.Name,
                tag.Keywords,
                tag.Colour,
            });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update tag {TagId}", tag.Id);
            throw new RepositoryException("Failed to update the tag.", ex);
        }
    }

    public int Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Tags
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete tag {TagId}", id);
            throw new RepositoryException("Failed to delete the tag.", ex);
        }
    }
}
