using SmartCv.Services;

namespace SmartCv.Factories;

public interface IFontFactory
{
    IEnumerable<string> GetAll();
}

public class FontFactory : IFontFactory
{
    private readonly IFontService _fontService;

    public FontFactory(IFontService fontService)
    {
        _fontService = fontService;
    }

    public IEnumerable<string> GetAll() =>
        _fontService.GetInstalledFamilies()
            .Select(family => family.Trim())
            .Where(family => family.Length > 0 && !family.StartsWith('@'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
