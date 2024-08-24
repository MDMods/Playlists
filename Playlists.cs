using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MelonLoader;

namespace Playlists;

public class Playlists : MelonMod {
    public static MelonLogger.Instance Logger { get; } = new(nameof(Playlists));

    private static string PlaylistPath => Path.Combine(Directory.GetCurrentDirectory(), "UserData/Playlists");
        
    public static Dictionary<string, string> DefaultNames { get; } = new()
    {
        { "English", "Playlists" },
        { "ChineseS", "Playlists" },
        { "ChineseT", "Playlists" },
        { "Japanese", "Playlists" },
        { "Korean", "Playlists" }
    };

    public static List<CustomPlaylist> GetPlaylists()
    {
        var list = new List<CustomPlaylist>();
        
        EnsureCreated();

        var files = Directory.GetFiles(PlaylistPath, "*.json");
        
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            var json = File.ReadAllText(file);
            var playlist = JsonSerializer.Deserialize<CustomPlaylist>(json);

            playlist.FileName = Path.GetFileName(file);
            playlist.Creation = info.CreationTime;
            list.Add(playlist);
        }
        
        list.Sort((a, b) => a.CompareTo(b));
        return list;
    }

    private static void EnsureCreated()
    {
        if (Directory.Exists(PlaylistPath) && Directory.GetFiles(PlaylistPath, "*.json").Any())
            return;

        Directory.CreateDirectory(PlaylistPath);
        
        var defaultPlaylist = new CustomPlaylist
        {
            Name = "Custom Playlist",
            Icon = "https://mdmc.moe/cdn/melon.png",
            Position = 1,
            Albums = new List<string> { "0-47", "58-0", "58-2" }
        };

        var json = JsonSerializer.Serialize(defaultPlaylist, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(Path.Combine(PlaylistPath, "default.json"), json);
    }
}