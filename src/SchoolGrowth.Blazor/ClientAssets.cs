using System.Globalization;

namespace SchoolGrowth.Blazor;

public static class ClientAssets
{
    private static readonly string ResourceVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);

    public static string DataUrl(string path)
    {
        var normalized = path.TrimStart('/');
        var separator = normalized.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{normalized}{separator}v={ResourceVersion}";
    }
}
