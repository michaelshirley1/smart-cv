using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SmartCv.Host;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Tests;

public sealed class TestDatabase : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public TestDatabase()
    {
        Paths = new AppPaths(Path.Combine(Path.GetTempPath(), "smartcv-tests", Guid.NewGuid().ToString("N")));
        Paths.EnsureCreated();

        _provider = new ServiceCollection()
            .AddLogging()
            .AddSmartCv(Paths)
            .BuildServiceProvider();
        _scope = _provider.CreateScope();

        Get<ISchemaRepository>().EnsureCreated();
        Get<ITemplateFileService>().EnsureBuiltIns();
    }

    public AppPaths Paths { get; }
    public IDbConnectionFactory Connections => Get<IDbConnectionFactory>();

    public T Get<T>() where T : notnull => _scope.ServiceProvider.GetRequiredService<T>();

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        SqliteConnection.ClearAllPools();
        Directory.Delete(Paths.Root, true);
    }
}
