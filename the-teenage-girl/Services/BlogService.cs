namespace the_teenage_girl.Services;
using Models;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.AspNetCore.Hosting;

public class BlogService
{
    private readonly string _postsPath;
    private List<PostMetadata>? _cachedMeta;
    
    public BlogService(IWebHostEnvironment env)
    {
        _postsPath = Path.Combine(env.WebRootPath, "posts");
    }

    public Task<List<PostMetadata>> GetAllMetadataAsync()
    {
        if (_cachedMeta != null) return Task.FromResult(_cachedMeta);

        var files = Directory.GetFiles(_postsPath, "*.md");
        var result = new List<PostMetadata>();
        
        foreach (var file in files)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var raw = File.ReadAllText(file);
            var (meta, _) = Parse(raw, slug);
            result.Add(meta);
        }
        
        _cachedMeta = result.OrderByDescending(p => p.Date).ToList();
        return Task.FromResult(_cachedMeta);
    }

    public Task<BlogPost?> GetPostAsync(string slug)
    {
        var filePath = Path.Combine(_postsPath, $"{slug}.md");
        if (!File.Exists(filePath)) return Task.FromResult<BlogPost?>(null);
        
        var raw = File.ReadAllText(filePath);
        var (meta, htmlContent) = Parse(raw, slug);

        return Task.FromResult<BlogPost?>(new BlogPost
        {
            Slug = meta.Slug,
            Title = meta.Title,
            Date = meta.Date,
            Description = meta.Description,
            Image = meta.Image,
            Subtext = meta.Subtext,
            Category = meta.Category,
            HtmlContent = htmlContent
        });
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