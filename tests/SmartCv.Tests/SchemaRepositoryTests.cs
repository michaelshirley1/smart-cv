using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCv.Repositories;

namespace SmartCv.Tests;

public class SchemaRepositoryTests
{
    [Fact]
    public async Task EnsureCreated_CreatesAllTables()
    {
        using var database = new TestDatabase();
        using var connection = await database.Connections.Create();

        var tables = connection.Query<string>("SELECT name FROM sqlite_master WHERE type = 'table'").ToList();

        Assert.Superset(
            new HashSet<string> { "Profiles", "Sections", "Entries", "Lines", "Tags", "SectionTags", "LineTags", "Settings" },
            tables.ToHashSet());
    }

    [Fact]
    public async Task EnsureCreated_CanRunTwice()
    {
        using var database = new TestDatabase();
        var repository = new SchemaRepository(database.Connections, NullLogger<SchemaRepository>.Instance);

        await repository.EnsureCreated();
        using var connection = await database.Connections.Create();
        var settingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Settings");

        Assert.Equal(4, settingCount);
    }

    [Fact]
    public async Task ForeignKeys_AreEnforced()
    {
        using var database = new TestDatabase();
        using var connection = await database.Connections.Create();

        var enabled = connection.ExecuteScalar<int>("PRAGMA foreign_keys");

        Assert.Equal(1, enabled);
    }
}
