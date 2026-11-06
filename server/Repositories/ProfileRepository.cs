using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface IProfileRepository
{
    IEnumerable<Profile> GetAll();
    Profile? GetById(int id);
    int Create(Profile profile);
    int Update(Profile profile);
    int Delete(int id);
    int ReplaceTemplate(string currentName, string newName);
}

public class ProfileRepository : IProfileRepository
{
    private const string Columns = "Id, Name, TemplateName, TargetPages, FullName, Headline, Email, Phone, Location, Links, CreatedAt, UpdatedAt";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ProfileRepository> _logger;

    public ProfileRepository(IDbConnectionFactory connectionFactory, ILogger<ProfileRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IEnumerable<Profile> GetAll()
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Profiles
                ORDER BY UpdatedAt DESC, Id DESC
                """;
            using var connection = _connectionFactory.Create();
            var profiles = connection.Query<Profile>(sql).ToList();
            return profiles;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load profiles");
            throw new RepositoryException("Failed to load profiles.", ex);
        }
    }

    public Profile? GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Profiles
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var profile = connection.QuerySingleOrDefault<Profile>(sql, new { Id = id });
            return profile;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load profile {ProfileId}", id);
            throw new RepositoryException("Failed to load the profile.", ex);
        }
    }

    public int Create(Profile profile)
    {
        try
        {
            const string sql = """
                INSERT INTO Profiles (Name, TemplateName, TargetPages, FullName, Headline, Email, Phone, Location, Links)
                VALUES (@Name, @TemplateName, @TargetPages, @FullName, @Headline, @Email, @Phone, @Location, @Links)
                RETURNING Id
                """;
            using var connection = _connectionFactory.Create();
            var id = connection.ExecuteScalar<int>(sql, profile);
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create profile {ProfileName}", profile.Name);
            throw new RepositoryException("Failed to create the profile.", ex);
        }
    }

    public int Update(Profile profile)
    {
        try
        {
            const string sql = """
                UPDATE Profiles
                SET Name = @Name,
                    TemplateName = @TemplateName,
                    TargetPages = @TargetPages,
                    FullName = @FullName,
                    Headline = @Headline,
                    Email = @Email,
                    Phone = @Phone,
                    Location = @Location,
                    Links = @Links,
                    UpdatedAt = strftime('%Y-%m-%dT%H:%M:%SZ', 'now')
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, profile);
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update profile {ProfileId}", profile.Id);
            throw new RepositoryException("Failed to update the profile.", ex);
        }
    }

    public int Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Profiles
                WHERE Id = @Id
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete profile {ProfileId}", id);
            throw new RepositoryException("Failed to delete the profile.", ex);
        }
    }

    public int ReplaceTemplate(string currentName, string newName)
    {
        try
        {
            const string sql = """
                UPDATE Profiles
                SET TemplateName = @NewName
                WHERE TemplateName = @CurrentName
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql, new { CurrentName = currentName, NewName = newName });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to move profiles from template {TemplateName}", currentName);
            throw new RepositoryException("Failed to update profile templates.", ex);
        }
    }
}
