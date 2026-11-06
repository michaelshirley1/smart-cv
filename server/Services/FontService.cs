using SkiaSharp;

namespace SmartCv.Services;

public interface IFontService
{
    IReadOnlyList<string> GetInstalledFamilies();
}

public class FontService : IFontService
{
    public IReadOnlyList<string> GetInstalledFamilies() =>
        SKFontManager.Default.FontFamilies.ToList();
}
