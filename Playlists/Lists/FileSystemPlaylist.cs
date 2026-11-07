using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Playlists.Interfaces;

namespace Playlists.Lists;

public class FileSystemPlaylist : IPlaylist
{
    [JsonIgnore] public string ID { get; set; } = string.Empty; // filename
    [JsonIgnore] public DateTime Creation { get; set; }
    [JsonIgnore] public DateTime LastModified { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "";
    [JsonPropertyName("position")] public int Position { get; set; } = 1;
    [JsonPropertyName("albums")] public List<string> Albums { get; set; } = new();

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

    IEnumerable<string> IPlaylist.Resolve() => Playlists.ResolveReferences(Albums);
    int IComparable<IPlaylist>.CompareTo(IPlaylist? other) => Playlists.ComparePlaylists(this, other);

    #region R/W

    public void SaveToDisk()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(Path.Combine(Playlists.PlaylistPath, ID), json);
    }

    public static FileSystemPlaylist ReadFromDisk(string path)
    {
        var info = new FileInfo(path);
        var json = File.ReadAllText(path);
        var playlist = JsonSerializer.Deserialize<FileSystemPlaylist>(json)!;

        playlist.ID = Path.GetFileName(path);
        playlist.Creation = info.CreationTime;
        playlist.LastModified = info.LastWriteTime;
        return playlist;
    }

    #endregion
}