namespace the_teenage_girl.Models;

public class MovieMetaData
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public int Year { get; set; }
    public string Image { get; set; } = "";
    public DateTime DateWatched { get; set; }
    public float Rating { get; set; }
    public string Genre { get; set; } = "";
    public string Review { get; set; } = "";
    public string Description { get; set; } = "";
}