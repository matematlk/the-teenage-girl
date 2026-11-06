namespace the_teenage_girl.Services;
using Models;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Net.Http.Json;

public class BlogService
{
    private readonly HttpClient _http;
    private List<PostMetadata>? _cachedMeta;
    private Dictionary<string, string>? _rawCache; // slug
    
    public BlogService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PostMetadata>> GetAllMetadataAsync()
    {
        if (_cachedMeta != null) return _cachedMeta;

        var slugs = await _http.GetFromJsonAsync<List<string>>("posts/manifest.json") ?? new();
        var result = new List<PostMetadata>();
        _rawCache = new Dictionary<string, string>();
        
        foreach (var slug in slugs)
        {
            var raw = await _http.GetStringAsync($"posts/{slug}.md");
            _rawCache[slug] = raw;
            var (meta, _) = Parse(raw, slug);
            result.Add(meta);
        }
        
        _cachedMeta = result.OrderByDescending(p => p.Date).ToList();
        return _cachedMeta;
    }

    public async Task<BlogPost?> GetPostAsync(string slug)
    {
        string raw;
        if (_rawCache != null && _rawCache.TryGetValue(slug, out var cached))
        {
            raw = cached;
        }
        else
        {
            try
            {
                raw = await _http.GetStringAsync($"posts/{slug}.md");
            }
            catch (HttpRequestException)
            {
                return null; // 404 -> no such post
            }
        }
        var (meta, htmlContent) = Parse(raw, slug);

        return new BlogPost
        {
            Slug = meta.Slug,
            Title = meta.Title,
            Date = meta.Date,
            Description = meta.Description,
            Image = meta.Image,
            Subtext = meta.Subtext,
            Category = meta.Category,
            HtmlContent = htmlContent
        };
    }

    private (PostMetadata, string) Parse(string rawMarkdown, string slug)
    {
        var parts = rawMarkdown.Split(new[] { "---" }, 3, StringSplitOptions.None);

        PostMetadata meta = new() { Slug = slug };
        string body = rawMarkdown;

        if (parts.Length >= 3)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            
            meta = deserializer.Deserialize<PostMetadata>(parts[1]);
            meta.Slug = slug;
            body = parts[2];
        }
        
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(body, pipeline);
        
        return (meta, html);
    }
}