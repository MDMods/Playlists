using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Il2CppAccount;
using Il2CppInterop.Runtime;
using Il2CppNewtonsoft.Json;
using Il2CppNewtonsoft.Json.Linq;
using Il2CppSystem;
using MelonLoader;
using Playlists.Customs;
using UnityEngine;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Object = Il2CppSystem.Object;

namespace Playlists.Patches;

[HarmonyPatch(typeof(GameAccountSystem), nameof(GameAccountSystem.SendToUrl))]
public class APIPatch
{
    // ReSharper disable once InconsistentNaming
    private static bool Prefix(
        string url,
        string method,
        Il2CppSystem.Collections.Generic.Dictionary<string, Object> datas,
        Action<JObject> succeedCallback
    )
    {
        switch (url)
        {
            case "musedash/v1/music_tag":
            {
                Playlists.Logger.Msg("Caught music_tag request.");

                MelonCoroutines.Start(convertPlaylists(s =>
                {
                    try
                    {
                        Playlists.Logger.Msg($"Faking response to {s}.");
                        var obj = JsonConvert.DeserializeObject<JObject>(s);
                        succeedCallback.Invoke(obj);
                    }
                    finally
                    {
                        Playlists.FinishedLoading = true;
                        Playlists.Logger.Msg("Finished loading playlists.");
                    }
                }));

                return false;
            }
        }

        return true;
    }

    private static IEnumerator convertPlaylists(System.Action<string> callback)
    {
        var lists = new List<object>();

        for (var i = 0; i < Playlists.LoadedPlaylists.Count; i++)
        {
            var p = Playlists.LoadedPlaylists[i];
            var task = p.Resolve();

            yield return new WaitUntil(DelegateSupport.ConvertDelegate<Func<bool>>(() => task.IsCompleted));

            Playlists.ResolveCache[p] = task.Result;

            lists.Add(new
            {
                object_id = p.ID,
                created_at = p.Creation.ToString(CultureInfo.InvariantCulture),
                updated_at = p.LastModified.ToString(CultureInfo.InvariantCulture),
                tag_name = new Dictionary<string, string>
                {
                    { "English", p.Name },
                    { "ChineseS", p.Name },
                    { "ChineseT", p.Name },
                    { "Japanese", p.Name },
                    { "Korean", p.Name }
                },
                tag_picture = p.Icon,
                music_list = task.Result.Where(id => id != CustomsIntegration.FALLBACK),
                anchor_pattern = false,
                sort_key = i,
                icon_name = $"Icon{p.Name}"
            });
        }

        var json = JsonSerializer.Serialize(new
        {
            code = 0,
            music_tag_list = lists
        });

        callback.Invoke(json);
    }
}
