using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Il2CppNewtonsoft.Json;
using Il2CppNewtonsoft.Json.Linq;
using Playlists.Interfaces;

namespace Playlists.Customs;

public class CustomsRankedPlaylist : IPlaylist
{
    public string ID => "mdmc-ranked";
    public DateTime Creation => DateTime.MinValue;
    public DateTime LastModified => DateTime.MinValue;
    public string Name => "MDMC Ranked";
    public string Icon => "https://mdmc.moe/cdn/melon.png";
    public int Position => 0;
    public List<string> Albums { get; } = new();

    public bool Add(string album) => false;
    public bool Remove(string album) => false;

    public void SaveToDisk() { }

    private List<string>? ids;

    public async Task<IEnumerable<string>> Resolve()
    {
        if (ids is not null)
            return ids;

        var albums = CustomsIntegration.GetAllAlbums();
        ids = new List<string>();

        var cachePath = Path.Combine(Directory.GetCurrentDirectory(), "UserData/playlists-cache.json");
        var cache = File.Exists(cachePath)
            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(await File.ReadAllTextAsync(cachePath))
            : new Dictionary<string, CacheEntry>();
        cache ??= new Dictionary<string, CacheEntry>();

        // ReSharper disable once ShortLivedHttpClient
        var http = new HttpClient();

        foreach (var album in albums)
        {
            if (album.AlbumName.EndsWith("_folder"))
                continue;

            try
            {
                var uid = CustomsIntegration.GetIDForAlbum(album.AlbumName);
                var hash = album.Sheets.First().Value.Md5;

                if (cache.TryGetValue(hash, out var entry))
                {
                    if (entry.Ranked)
                    {
                        ids.Add(uid);
                        continue;
                    }

                    if (entry.LastChecked != null && DateTime.UtcNow - entry.LastChecked.Value < TimeSpan.FromDays(3))
                        continue;
                }

                var url = $"https://api.mdmc.moe/v3/sheets/{hash}";
                Playlists.Logger.Msg($"Checking ranked status of '{album.AlbumName} ({hash})'...");

                cache[hash] = entry = new CacheEntry { LastChecked = DateTime.UtcNow };

                var res = await http.GetAsync(url);

                if (!res.IsSuccessStatusCode)
                    continue;

                var raw = await res.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<JObject>(raw);
                var ranked = json["chart"]["ranked"].ToObject<bool>();

                if (!ranked)
                    continue;

                entry.LastChecked = null;
                entry.Ranked = true;
                ids.Add(uid);
            }
            catch (Exception ex)
            {
                Playlists.Logger.Error(ex);
            }
        }

        await File.WriteAllTextAsync(cachePath, System.Text.Json.JsonSerializer.Serialize(cache));
        return ids;
    }

    public int CompareTo(IPlaylist? other) => Playlists.ComparePlaylists(this, other);

    private class CacheEntry
    {
        public bool Ranked { get; set; }
        public DateTime? LastChecked { get; set; }
    }
}
