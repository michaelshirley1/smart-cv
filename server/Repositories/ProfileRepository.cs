using Dapper;
using Microsoft.Data.Sqlite;
using SmartCv.Models;

namespace SmartCv.Repositories;

public interface IProfileRepository
{
    Task<IEnumerable<Profile>> GetAll();
    Task<Profile?> GetById(int id);
    Task<int> Create(Profile profile);
    Task<int> Update(Profile profile);
    Task<int> Delete(int id);
    Task<int> ReplaceTemplate(string currentName, string newName);
    Task<int> SaveJobDescription(int id, string jobDescription);
}

public class ProfileRepository : IProfileRepository
{
    private const string Columns = "Id, Name, TemplateName, TargetPages, FullName, Headline, Email, Phone, Location, Links, JobDescription, CreatedAt, UpdatedAt";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ProfileRepository> _logger;

    public ProfileRepository(IDbConnectionFactory connectionFactory, ILogger<ProfileRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Profile>> GetAll()
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Profiles
                ORDER BY UpdatedAt DESC, Id DESC
                """;
            await using var connection = await _connectionFactory.Create();
            var profiles = (await connection.QueryAsync<Profile>(sql)).ToList();
            return profiles;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load profiles");
            throw new RepositoryException("Failed to load profiles.", ex);
        }
    }

    public async Task<Profile?> GetById(int id)
    {
        try
        {
            const string sql = $"""
                SELECT {Columns}
                FROM Profiles
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var profile = await connection.QuerySingleOrDefaultAsync<Profile>(sql, new { Id = id });
            return profile;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to load profile {ProfileId}", id);
            throw new RepositoryException("Failed to load the profile.", ex);
        }
    }

    public async Task<int> Create(Profile profile)
    {
        try
        {
            const string sql = """
                INSERT INTO Profiles (Name, TemplateName, TargetPages, FullName, Headline, Email, Phone, Location, Links)
                VALUES (@Name, @TemplateName, @TargetPages, @FullName, @Headline, @Email, @Phone, @Location, @Links)
                RETURNING Id
                """;
            await using var connection = await _connectionFactory.Create();
            var id = await connection.ExecuteScalarAsync<int>(sql, profile);
            return id;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create profile {ProfileName}", profile.Name);
            throw new RepositoryException("Failed to create the profile.", ex);
        }
    }

    public async Task<int> Update(Profile profile)
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
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, profile);
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to update profile {ProfileId}", profile.Id);
            throw new RepositoryException("Failed to update the profile.", ex);
        }
    }

    public async Task<int> Delete(int id)
    {
        try
        {
            const string sql = """
                DELETE FROM Profiles
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to delete profile {ProfileId}", id);
            throw new RepositoryException("Failed to delete the profile.", ex);
        }
    }

    public async Task<int> ReplaceTemplate(string currentName, string newName)
    {
        try
        {
            const string sql = """
                UPDATE Profiles
                SET TemplateName = @NewName
                WHERE TemplateName = @CurrentName
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new { CurrentName = currentName, NewName = newName });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to move profiles from template {TemplateName}", currentName);
            throw new RepositoryException("Failed to update profile templates.", ex);
        }
    }

    public async Task<int> SaveJobDescription(int id, string jobDescription)
    {
        try
        {
            const string sql = """
                UPDATE Profiles
                SET JobDescription = @JobDescription
                WHERE Id = @Id
                """;
            await using var connection = await _connectionFactory.Create();
            var affected = await connection.ExecuteAsync(sql, new { Id = id, JobDescription = jobDescription });
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to save the job description for profile {ProfileId}", id);
            throw new RepositoryException("Failed to save the job description.", ex);
        }
    }
}
