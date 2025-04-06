public class CachedItem
{
    public string ResponseBody { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.Now;
}