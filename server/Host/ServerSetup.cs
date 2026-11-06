using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using SmartCv.Factories;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Host;

public static class ServerSetup
{
    public static WebApplication Build(string[] args, AppOptions options, IAppPaths paths)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.WebHost.UseUrls(options.ServerUrl);

        builder.Services.AddControllers()
            .AddJsonOptions(json => json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase)));
        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(paths);
        builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        builder.Services.AddSingleton<ITemplateFileService, TemplateFileService>();

        builder.Services.AddScoped<ISchemaRepository, SchemaRepository>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

        builder.Services.AddScoped<ISettingsFactory, SettingsFactory>();

        var app = builder.Build();

        app.UseExceptionHandler(error => error.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var message = exception is RepositoryException ? exception.Message : "Something went wrong.";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message });
        }));

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(paths.Templates),
            RequestPath = "/templates",
            OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-store",
        });

        var client = ClientFiles();
        if (client is not null)
        {
            app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = client });
            app.UseStaticFiles(new StaticFileOptions { FileProvider = client });
        }

        app.MapControllers();

        if (client is not null)
        {
            app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = client });
        }

        return app;
    }

    private static IFileProvider? ClientFiles()
    {
        var embedded = new ManifestEmbeddedFileProvider(typeof(ServerSetup).Assembly);
        var client = embedded.GetDirectoryContents("wwwroot");
        return client.Exists ? new ManifestEmbeddedFileProvider(typeof(ServerSetup).Assembly, "wwwroot") : null;
    }
}
