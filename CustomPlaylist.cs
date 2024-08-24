using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Playlists;

public class CustomPlaylist : IComparable<CustomPlaylist>
{
    [JsonIgnore]
    public string FileName { get; set; }
    
    [JsonIgnore]
    public DateTime Creation { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "";
    
    [JsonPropertyName("position")]
    public int Position { get; set; } = 1;

    [JsonPropertyName("albums")]
    public List<string> Albums { get; set; } = new();

    public int CompareTo(CustomPlaylist other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        
        if (other is null)
            return 1;

        var result = Position.CompareTo(other.Position);
        return result != 0 ? result : string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
}