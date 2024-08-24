using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Il2CppAccount;
using Il2CppNewtonsoft.Json;
using Il2CppNewtonsoft.Json.Linq;
using Il2CppSystem;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Playlists.Patches;

[HarmonyPatch(typeof(GameAccountSystem), nameof(GameAccountSystem.SendToUrl))]
public class APIPatch
{
    private static bool Prefix(string url, string method,
        Il2CppSystem.Collections.Generic.Dictionary<string, Object> datas, Action<JObject> succeedCallback)
    {

        switch (url)
        {
            case "musedash/v1/music_tag":
            {
                Playlists.Logger.Msg("Caught music_tag request.");

                var playlists = Playlists.GetPlaylists();

                var data = new
                {
                    code = 0,
                    music_tag_list = playlists.Select((p, i) => new
                    {
                        object_id = p.FileName,
                        created_at = p.Creation.ToString(CultureInfo.InvariantCulture),
                        updated_at = p.Creation.ToString(CultureInfo.InvariantCulture),
                        tag_name = new Dictionary<string, string>
                        {
                            { "English", p.Name },
                            { "ChineseS", p.Name },
                            { "ChineseT", p.Name },
                            { "Japanese", p.Name },
                            { "Korean", p.Name }
                        },
                        tag_picture = p.Icon,
                        music_list = p.Albums,
                        anchor_pattern = false,
                        sort_key = i,
                        icon_name = $"Icon{p.Name}"
                    })
                };

                var json = JsonSerializer.Serialize(data);
                Playlists.Logger.Msg(json);
                var obj = JsonConvert.DeserializeObject<JObject>(json);
                succeedCallback?.Invoke(obj);
                return false;
            }
        }
        
        return true;
    }
}