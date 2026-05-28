using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Gets the items from <see cref="Albums"/> and resolves any customs references.
    /// (Ignores album_ prefixes if customs isn't loaded.)
    /// </summary>
    /// <returns>A list of MusicUids</returns>
    public IEnumerable<string> Resolve()
    {
        foreach (var alb in Albums)
        {
            if (!alb.StartsWith(CustomsIntegration.AlbumPrefix))
                yield return alb;

            if (!Playlists.CustomAlbumsInstalled)
            {
                Playlists.Logger.Error($"Failed to resolve custom album '{alb}' because CustomAlbums is missing!");
                continue;
            }

            var result = CustomsIntegration.GetIDForAlbum(alb);
            if (result == CustomsIntegration.Fallback) continue;

            yield return result;
        }
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

    #region R/W

    public void SaveToDisk()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(Path.Combine(Playlists.PlaylistPath, FileName), json);
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

    #endregion
}