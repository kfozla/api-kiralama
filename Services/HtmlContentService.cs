using Ganss.Xss;

namespace api_kiralama.Services;

public class HtmlContentService
{
    private const int MaxLength = 100_000;

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        if (html.Length > MaxLength)
            throw new ArgumentException(
                "HTML içeriği 100.000 karakteri aşamaz."
            );

        var sanitizer = new HtmlSanitizer();

        // İzin verilen etiketleri sınırlandır.
        sanitizer.AllowedTags.Clear();

        foreach (var tag in new[]
                 {
                     "p", "br", "strong", "b", "em", "i",
                     "u", "s", "h1", "h2", "h3", "h4",
                     "ul", "ol", "li", "blockquote",
                     "a", "span", "div"
                 })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();

        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("title");

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("mailto");

        sanitizer.AllowDataAttributes = false;

        return sanitizer.Sanitize(html);
    }
}