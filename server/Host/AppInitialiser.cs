using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Host;

public static class AppInitialiser
{
    public static void Run(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ISchemaRepository>().EnsureCreated();
        scope.ServiceProvider.GetRequiredService<ITemplateFileService>().EnsureBuiltIns();
    }
}
