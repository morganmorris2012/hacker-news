using System.Text.Json.Serialization;

namespace HackerNewsReader.Core.Models;

public class Story
{
    private string _title = string.Empty;
    private string? _url;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty", nameof(value));
            _title = value;
        }
    }

    [JsonPropertyName("url")]
    public string? Url
    {
        get => _url;
        set
        {
            if (value != null && !Uri.TryCreate(value, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid URL format", nameof(value));
            _url = value;
        }
    }

    [JsonPropertyName("by")]
    public string By { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("descendants")]
    public int Descendants { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "story";
}