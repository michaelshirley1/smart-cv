using Microsoft.Extensions.Logging.Abstractions;
using SmartCv.Host;
using SmartCv.Repositories;

namespace SmartCv.Tests;

public sealed class TestDatabase : IDisposable
{
    public TestDatabase()
    {
        Paths = new AppPaths(Path.Combine(Path.GetTempPath(), "smartcv-tests", Guid.NewGuid().ToString("N")));
        Paths.EnsureCreated();
        Connections = new DbConnectionFactory(Paths);
        new SchemaRepository(Connections, NullLogger<SchemaRepository>.Instance).EnsureCreated();
    }

    public AppPaths Paths { get; }
    public IDbConnectionFactory Connections { get; }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(Paths.Root, true);
    }
}
