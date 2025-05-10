using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace Catalog.API.Extensions;

public static class StringExtensions
{
    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Guid.NewGuid().ToString();
        }

        var slug = name.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(ch => char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (sb, ch) => sb.Append(ch))
            .ToString();

        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString() : slug;
    }

    public static string NormalizeTag(this string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return string.Empty;

        var normalized = tag.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(ch => char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (sb, ch) => sb.Append(ch))
            .ToString();

        normalized = Regex.Replace(normalized, @"\s+", "-");
        normalized = Regex.Replace(normalized, @"[^a-z0-9-]", "");
        normalized = Regex.Replace(normalized, @"-+", "-");
        normalized = normalized.Trim('-');

        return normalized;
    }
}
