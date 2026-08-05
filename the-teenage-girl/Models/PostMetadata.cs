namespace the_teenage_girl.Models;
public class PostMetadata
{
    public string Slug { get; set; } = "";       // derived from filename
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime Date { get; set; }
    public string Image { get; set; } = "";
    public string Description { get; set; } = "";
    public string Subtext { get; set; } = "";
}