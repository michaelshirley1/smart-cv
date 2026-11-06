using SmartCv.Factories;
using SmartCv.Services;

namespace SmartCv.Tests;

public class FontFactoryTests
{
    private class FakeFontService(params string[] families) : IFontService
    {
        public IReadOnlyList<string> GetInstalledFamilies() => families;
    }

    [Fact]
    public void GetAll_SortsRemovesDuplicatesAndVerticalFonts()
    {
        var factory = new FontFactory(new FakeFontService("Garamond", "arial", "@MS Gothic", "Arial", " ", "Calibri"));

        Assert.Equal(["arial", "Calibri", "Garamond"], factory.GetAll());
    }

    [Fact]
    public void FontService_FindsInstalledFonts()
    {
        var families = new FontService().GetInstalledFamilies();

        Assert.NotEmpty(families);
    }
}
