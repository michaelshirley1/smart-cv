using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class TemplateFactoryTests
{
    [Fact]
    public void GetAll_ListsBuiltIns()
    {
        using var database = new TestDatabase();

        var templates = database.Get<ITemplateFactory>().GetAll().ToList();

        Assert.Equal(["compact", "default", "modern"], templates.Select(t => t.Name));
        Assert.All(templates, template => Assert.True(template.IsBuiltIn));
    }

    [Fact]
    public void GetByName_ReadsDefaultTemplateValues()
    {
        using var database = new TestDatabase();

        var template = database.Get<ITemplateFactory>().GetByName(TemplateSummary.DefaultName).Value!;

        Assert.Equal("\"Garamond\", \"EB Garamond\", \"Cormorant Garamond\", serif", template.Values["body-font"]);
        Assert.Equal("\"\\2756\"", template.Values["contact-separator"]);
        Assert.Equal("8.5in", template.Values["page-width"]);
    }

    [Fact]
    public void Create_SlugifiesNameAndFillsMissingValuesFromDefault()
    {
        using var database = new TestDatabase();
        var factory = database.Get<ITemplateFactory>();

        var result = factory.Create(new TemplateRequest
        {
            Name = "My Tokyo CV!",
            Values = new() { ["accent-colour"] = "#b3261e", ["body-font"] = "\"Noto Serif JP\", serif" },
        });

        Assert.Equal(FactoryStatus.Success, result.Status);
        var saved = factory.GetByName("my-tokyo-cv").Value!;
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
    public void Create_RejectsUnsafeOrUnknownValues(string key, string value)
    {
        using var database = new TestDatabase();

        var result = database.Get<ITemplateFactory>().Create(new TemplateRequest { Name = "bad", Values = new() { [key] = value } });

        Assert.Equal(FactoryStatus.Invalid, result.Status);
    }

    [Fact]
    public void BuiltIns_AreReadOnly()
    {
        using var database = new TestDatabase();
        var factory = database.Get<ITemplateFactory>();

        var update = factory.Update(TemplateSummary.DefaultName, new TemplateRequest { Name = "default", Values = new() { ["font-size"] = "12pt" } });
        var delete = factory.Delete(TemplateSummary.DefaultName);
        var duplicateName = factory.Create(new TemplateRequest { Name = "Default", Values = [] });

        Assert.Equal(FactoryStatus.Invalid, update.Status);
        Assert.Equal(FactoryStatus.Invalid, delete.Status);
        Assert.Equal(FactoryStatus.Invalid, duplicateName.Status);
    }

    [Fact]
    public void RenameAndDelete_MoveProfilesToTheRightTemplate()
    {
        using var database = new TestDatabase();
        var templates = database.Get<ITemplateFactory>();
        var profiles = database.Get<IProfileFactory>();
        templates.Create(new TemplateRequest { Name = "draft", Values = [] });
        var profile = profiles.Create(new ProfileRequest { Name = "Engineering", TemplateName = "draft" }).Value!;

        var renamed = templates.Update("draft", new TemplateRequest { Name = "final", Values = new() { ["font-size"] = "10pt" } });
        Assert.Equal(FactoryStatus.Success, renamed.Status);
        Assert.Equal("final", profiles.GetById(profile.Id).Value!.TemplateName);
        Assert.Equal(FactoryStatus.NotFound, templates.GetByName("draft").Status);
        Assert.Equal("10pt", templates.GetByName("final").Value!.Values["font-size"]);

        Assert.Equal(FactoryStatus.Success, templates.Delete("final").Status);
        Assert.Equal(TemplateSummary.DefaultName, profiles.GetById(profile.Id).Value!.TemplateName);
    }

    [Theory]
    [InlineData("../secrets")]
    [InlineData("..\\secrets")]
    [InlineData("Default")]
    public void UnsafeNames_AreNotFound(string name)
    {
        using var database = new TestDatabase();
        var factory = database.Get<ITemplateFactory>();

        Assert.Equal(FactoryStatus.NotFound, factory.GetByName(name).Status);
        Assert.Equal(FactoryStatus.NotFound, factory.Delete(name).Status);
    }
}
