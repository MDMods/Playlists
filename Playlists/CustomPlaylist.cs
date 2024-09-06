using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;

namespace Playlists;

public class CustomPlaylist : IComparable<CustomPlaylist>
{
    [JsonIgnore]
    public string FileName { get; set; }
    
    [JsonIgnore]
    public DateTime Creation { get; set; }
    
    [JsonIgnore]
    public DateTime LastModified { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "";
    
    [JsonPropertyName("position")]
    public int Position { get; set; } = 1;

    [JsonPropertyName("albums")]
    public List<string> Albums { get; set; } = new();

    public bool Add(string album)
    {
        if (Albums.Contains(album))
            return false;
        
        Albums.Insert(0, album);
        return true;
    }

    public bool Remove(string album)
    {
        if (!Albums.Contains(album))
            return false;
        
        Albums.Remove(album);
        return true;
    }

    public void SaveToDisk()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(Path.Combine(Playlists.PlaylistPath, FileName), json);
    }

    public int CompareTo(CustomPlaylist other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        
        if (other is null)
            return 1;

        var result = Position.CompareTo(other.Position);
        return result != 0 ? result : string.Compare(Name, other.Name, StringComparison.Ordinal);
    }

    public static CustomPlaylist ReadFromDisk(string path)
    {
        var info = new FileInfo(path);
        var json = File.ReadAllText(path);
        var playlist = JsonSerializer.Deserialize<CustomPlaylist>(json);

        playlist.FileName = Path.GetFileName(path);
        playlist.Creation = info.CreationTime;
        playlist.LastModified = info.LastWriteTime;
        return playlist;
    }
}