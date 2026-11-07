using Dapper;
using Microsoft.Data.Sqlite;

namespace SmartCv.Repositories;

public interface ISchemaRepository
{
    int EnsureCreated();
}

public class SchemaRepository : ISchemaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SchemaRepository> _logger;

    public SchemaRepository(IDbConnectionFactory connectionFactory, ILogger<SchemaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public int EnsureCreated()
    {
        try
        {
            const string sql = """
                CREATE TABLE IF NOT EXISTS Profiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    TemplateName TEXT NOT NULL DEFAULT 'default',
                    TargetPages INTEGER NOT NULL DEFAULT 1,
                    FullName TEXT NOT NULL DEFAULT '',
                    Headline TEXT NOT NULL DEFAULT '',
                    Email TEXT NOT NULL DEFAULT '',
                    Phone TEXT NOT NULL DEFAULT '',
                    Location TEXT NOT NULL DEFAULT '',
                    Links TEXT NOT NULL DEFAULT '[]',
                    JobDescription TEXT NOT NULL DEFAULT '',
                    CreatedAt TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ', 'now')),
                    UpdatedAt TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ', 'now'))
                );

                CREATE TABLE IF NOT EXISTS Sections (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProfileId INTEGER NOT NULL REFERENCES Profiles (Id) ON DELETE CASCADE,
                    Title TEXT NOT NULL,
                    Kind TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    MaxLines INTEGER NOT NULL DEFAULT 5,
                    IsHidden INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Entries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SectionId INTEGER NOT NULL REFERENCES Sections (Id) ON DELETE CASCADE,
                    Heading TEXT NOT NULL DEFAULT '',
                    Subheading TEXT NOT NULL DEFAULT '',
                    Location TEXT NOT NULL DEFAULT '',
                    DateRange TEXT NOT NULL DEFAULT '',
                    SortOrder INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Lines (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryId INTEGER NOT NULL REFERENCES Entries (Id) ON DELETE CASCADE,
                    Text TEXT NOT NULL DEFAULT '',
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    Override TEXT NOT NULL DEFAULT 'none'
                );

                CREATE TABLE IF NOT EXISTS Tags (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProfileId INTEGER NOT NULL REFERENCES Profiles (Id) ON DELETE CASCADE,
                    Name TEXT NOT NULL,
                    Keywords TEXT NOT NULL DEFAULT '[]',
                    Colour TEXT NOT NULL DEFAULT '',
                    IsAlwaysOn INTEGER NOT NULL DEFAULT 0,
                    UNIQUE (ProfileId, Name)
                );

                CREATE TABLE IF NOT EXISTS SectionTags (
                    SectionId INTEGER NOT NULL REFERENCES Sections (Id) ON DELETE CASCADE,
                    TagId INTEGER NOT NULL REFERENCES Tags (Id) ON DELETE CASCADE,
                    PRIMARY KEY (SectionId, TagId)
                );

                CREATE TABLE IF NOT EXISTS LineTags (
                    LineId INTEGER NOT NULL REFERENCES Lines (Id) ON DELETE CASCADE,
                    TagId INTEGER NOT NULL REFERENCES Tags (Id) ON DELETE CASCADE,
                    PRIMARY KEY (LineId, TagId)
                );

                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS IX_Sections_ProfileId ON Sections (ProfileId);
                CREATE INDEX IF NOT EXISTS IX_Entries_SectionId ON Entries (SectionId);
                CREATE INDEX IF NOT EXISTS IX_Lines_EntryId ON Lines (EntryId);
                CREATE INDEX IF NOT EXISTS IX_Tags_ProfileId ON Tags (ProfileId);
                CREATE INDEX IF NOT EXISTS IX_SectionTags_TagId ON SectionTags (TagId);
                CREATE INDEX IF NOT EXISTS IX_LineTags_TagId ON LineTags (TagId);

                INSERT OR IGNORE INTO Settings (Key, Value) VALUES
                    ('MatchingMode', 'keyword'),
                    ('OllamaUrl', 'http://127.0.0.1:11434'),
                    ('OllamaModel', ''),
                    ('LastProfileId', '');

                UPDATE Settings
                SET Value = 'http://127.0.0.1:11434'
                WHERE Key = 'OllamaUrl' AND Value = 'http://localhost:11434';
                """;
            const string columnSql = """
                SELECT COUNT(*)
                FROM pragma_table_info('Profiles')
                WHERE name = 'JobDescription'
                """;
            const string addColumnSql = """
                ALTER TABLE Profiles
                ADD COLUMN JobDescription TEXT NOT NULL DEFAULT ''
                """;
            using var connection = _connectionFactory.Create();
            var affected = connection.Execute(sql);
            if (connection.ExecuteScalar<int>(columnSql) == 0) affected += connection.Execute(addColumnSql);
            return affected;
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Failed to create the database schema");
            throw new RepositoryException("Failed to create the database.", ex);
        }
    }
}
