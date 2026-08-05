namespace the_teenage_girl.Services;
using Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.AspNetCore.Hosting;

public class MovieService
{
    private readonly string _moviesPath;
    private List<MovieMetaData>? _cachedMovies;

    // STEP 1: constructor
    // build path to wwwroot/movies/ and store it
    public MovieService(IWebHostEnvironment env)
    {
        _moviesPath = Path.Combine(env.WebRootPath, "movies");
        Directory.CreateDirectory(_moviesPath);
    }

    // STEP 2: get all movies
    // read all files once, store result, and return cached list on subsequent calls
    public Task<List<MovieMetaData>> GetAllMoviesAsync()
    {
        if (_cachedMovies != null) return Task.FromResult(_cachedMovies);

        var files = Directory.GetFiles(_moviesPath, "*.md");
        var result = new List<MovieMetaData>();

        foreach (var file in files)
        {
            try
            {
                var slug = Path.GetFileNameWithoutExtension(file);
                var raw = File.ReadAllText(file);
                var movie = Parse(raw, slug);
                result.Add(movie);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Skipping {file}: {ex.Message}");
            }
            
        }
        
        _cachedMovies = result.OrderByDescending(m => m.DateWatched).ToList();
        return Task.FromResult(_cachedMovies);
    }

    // STEP 3: get single movie by slug
    // used if ever want to add a page at /movies/{slug}
    public Task<MovieMetaData?> GetMovieAsync(string slug)
    {
        var filePath = Path.Combine(_moviesPath, $"{slug}.md");
        if (!File.Exists(filePath)) return Task.FromResult<MovieMetaData?>(null);
        
        var raw = File.ReadAllText(filePath);
        var movie = Parse(raw, slug);
        return Task.FromResult<MovieMetaData?>(movie);
    }
    
    // STEP4 : parse single file
    // get frontmatter only (because no content as opposed to blog)
    private MovieMetaData Parse(string rawMarkdown, string slug)
    {
        var parts = rawMarkdown.Split(new[] { "---" }, 3, StringSplitOptions.None);
        
        // if there is valid frontmatter (3 parts: empty, yaml, body)
        if (parts.Length >= 3)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var movie = deserializer.Deserialize<MovieMetaData>(parts[1]);
            movie.Slug = slug; // always override slug with filename, not frontmatter
            return movie;
        }
        
        // fallback if file has no frontmatter
        return new MovieMetaData { Slug = slug };
    }
}