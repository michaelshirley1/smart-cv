using SmartCv.Factories;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Host;

public static class ServiceRegistration
{
    public static IServiceCollection AddSmartCv(this IServiceCollection services, IAppPaths paths)
    {
        services.AddSingleton(paths);
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<ITemplateFileService, TemplateFileService>();
        services.AddSingleton<IFontService, FontService>();
        services.AddSingleton<IOllamaScoreCache, OllamaScoreCache>();
        services.AddHttpClient<IOllamaService, OllamaService>();
        services.AddSingleton<IPrintJobStore, PrintJobStore>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IClientAddress, ClientAddress>();
        services.AddSingleton<IDocumentReader, DocxReader>();
        services.AddSingleton<IDocumentReader, PdfReader>();
        services.AddSingleton<IDocumentReader, PlainTextReader>();

        services.AddScoped<ISchemaRepository, SchemaRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<IEntryRepository, EntryRepository>();
        services.AddScoped<ILineRepository, LineRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.AddScoped<ISettingsFactory, SettingsFactory>();
        services.AddScoped<IProfileFactory, ProfileFactory>();
        services.AddScoped<ISectionFactory, SectionFactory>();
        services.AddScoped<IEntryFactory, EntryFactory>();
        services.AddScoped<ILineFactory, LineFactory>();
        services.AddScoped<ITagFactory, TagFactory>();
        services.AddScoped<ITemplateFactory, TemplateFactory>();
        services.AddScoped<IFontFactory, FontFactory>();
        services.AddScoped<ITailoringFactory, TailoringFactory>();
        services.AddScoped<IExportFactory, ExportFactory>();
        services.AddScoped<IImportFactory, ImportFactory>();

        return services;
    }
}
