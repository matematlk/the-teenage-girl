namespace the_teenage_girl.Services;
using Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Net.Http.Json;

public class MovieService
{
    private readonly HttpClient _http;
    private List<MovieMetaData>? _cachedMovies;

    public MovieService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MovieMetaData>> GetAllMoviesAsync()
    {
        if (_cachedMovies != null) return _cachedMovies;

        var slugs = await _http.GetFromJsonAsync<List<string>>("movies/manifest.json") ?? new();
        var result = new List<MovieMetaData>();

        foreach (var slug in slugs)
        {
            try
            {
                var raw = await _http.GetStringAsync($"movies/{slug}.md");
                var movie = Parse(raw, slug);
                result.Add(movie);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Skipping {slug}: {ex.Message}");
            }
            
        }
        
        _cachedMovies = result.OrderByDescending(m => m.DateWatched).ToList();
        return _cachedMovies;
    }

    public async Task<MovieMetaData?> GetMovieAsync(string slug)
    {
        try
        {
            var raw = await _http.GetStringAsync($"movies/{slug}.md");
            return Parse(raw, slug);
        }
        catch (HttpRequestException)
        {
            return null; // 404 no movie
        }
    }
    
    private MovieMetaData Parse(string rawMarkdown, string slug)
    {
        var parts = rawMarkdown.Split(new[] { "---" }, 3, StringSplitOptions.None);
        
        if (parts.Length >= 3)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var movie = deserializer.Deserialize<MovieMetaData>(parts[1]);
            movie.Slug = slug;
            return movie;
        }
        
        return new MovieMetaData { Slug = slug };
    }
}