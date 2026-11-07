using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class TemplateFactoryTests
{
    [Fact]
    public async Task GetAll_ListsBuiltIns()
    {
        using var database = new TestDatabase();

        var templates = (await database.Get<ITemplateFactory>().GetAll()).ToList();

        Assert.Equal(["compact", "default", "modern"], templates.Select(t => t.Name));
        Assert.All(templates, template => Assert.True(template.IsBuiltIn));
    }

    [Fact]
    public async Task GetByName_ReadsDefaultTemplateValues()
    {
        using var database = new TestDatabase();

        var template = (await database.Get<ITemplateFactory>().GetByName(TemplateSummary.DefaultName)).Value!;

        Assert.Equal("\"Garamond\", \"EB Garamond\", \"Cormorant Garamond\", serif", template.Values["body-font"]);
        Assert.Equal("\"\\2756\"", template.Values["contact-separator"]);
        Assert.Equal("8.5in", template.Values["page-width"]);
    }

    [Fact]
    public async Task Create_SlugifiesNameAndFillsMissingValuesFromDefault()
    {
        using var database = new TestDatabase();
        var factory = database.Get<ITemplateFactory>();

        var result = await factory.Create(new TemplateRequest
        {
            Name = "My Tokyo CV!",
            Values = new() { ["accent-colour"] = "#b3261e", ["body-font"] = "\"Noto Serif JP\", serif" },
        });

        Assert.Equal(FactoryStatus.Success, result.Status);
        var saved = (await factory.GetByName("my-tokyo-cv")).Value!;
        Assert.False(saved.IsBuiltIn);
        Assert.Equal("#b3261e", saved.Values["accent-colour"]);
        Assert.Equal("\"Noto Serif JP\", serif", saved.Values["body-font"]);
        Assert.Equal("24pt", saved.Values["name-size"]);
        Assert.True(File.Exists(Path.Combine(database.Paths.Templates, "my-tokyo-cv.css")));
    }

    [Theory]
    [InlineData("unknown-setting", "1pt")]
    [InlineData("accent-colour", "red; } body { display: none")]
    [InlineData("accent-colour", "")]
    [InlineData("body-font", "<script>")]
    public async Task Create_RejectsUnsafeOrUnknownValues(string key, string value)
    {
        using var database = new TestDatabase();

        var result = await database.Get<ITemplateFactory>().Create(new TemplateRequest { Name = "bad", Values = new() { [key] = value } });

        Assert.Equal(FactoryStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task BuiltIns_AreReadOnly()
    {
        using var database = new TestDatabase();
        var factory = database.Get<ITemplateFactory>();

        var update = await factory.Update(TemplateSummary.DefaultName, new TemplateRequest { Name = "default", Values = new() { ["font-size"] = "12pt" } });
        var delete = await factory.Delete(TemplateSummary.DefaultName);
        var duplicateName = await factory.Create(new TemplateRequest { Name = "Default", Values = [] });

        Assert.Equal(FactoryStatus.Invalid, update.Status);
        Assert.Equal(FactoryStatus.Invalid, delete.Status);
        Assert.Equal(FactoryStatus.Invalid, duplicateName.Status);
    }

    [Fact]
    public async Task RenameAndDelete_MoveProfilesToTheRightTemplate()
    {
        using var database = new TestDatabase();
        var templates = database.Get<ITemplateFactory>();
        var profiles = database.Get<IProfileFactory>();
        await templates.Create(new TemplateRequest { Name = "draft", Values = [] });
        var profile = (await profiles.Create(new ProfileRequest { Name = "Engineering", TemplateName = "draft" })).Value!;

        var renamed = await templates.Update("draft", new TemplateRequest { Name = "final", Values = new() { ["font-size"] = "10pt" } });
        Assert.Equal(FactoryStatus.Success, renamed.Status);
        Assert.Equal("final", (await profiles.GetById(profile.Id)).Value!.TemplateName);
        Assert.Equal(FactoryStatus.NotFound, (await templates.GetByName("draft")).Status);
        Assert.Equal("10pt", (await templates.GetByName("final")).Value!.Values["font-size"]);

        Assert.Equal(FactoryStatus.Success, (await templates.Delete("final")).Status);
        Assert.Equal(TemplateSummary.DefaultName, (await profiles.GetById(profile.Id)).Value!.TemplateName);
    }

    [Theory]
    [InlineData("../secrets")]
    [InlineData("..\\secrets")]
    [InlineData("Default")]
    public async Task UnsafeNames_AreNotFound(string name)
    {
        using var database = new TestDatabase();
        var factory = database.Get<ITemplateFactory>();

        Assert.Equal(FactoryStatus.NotFound, (await factory.GetByName(name)).Status);
        Assert.Equal(FactoryStatus.NotFound, (await factory.Delete(name)).Status);
    }
}
