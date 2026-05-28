using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppSystem.Collections.Generic;

namespace Playlists.Patches;

[HarmonyPatch(typeof(PnlMusicTagItem), nameof(PnlMusicTagItem.GetMusicInfos))]
public static class TagPatch
{
    private const int CustomTagStartIdx = 32768;

// ReSharper disable InconsistentNaming
    public static void Postfix(PnlMusicTagItem __instance, int index,
        ref List<MusicInfo> __result)
// ReSharper enable InconsistentNaming
    {
        // is it stupid? yea
        // does it work? yea
        var localIdx = __instance.m_CurTagIndex - CustomTagStartIdx;

        if (localIdx < 0 || Playlists.LoadedPlaylists.Count <= localIdx)
            return;

        __result.Clear();
        var playlist = Playlists.LoadedPlaylists[localIdx];

        // Playlists.Logger.Msg($"{__instance.m_CurTagIndex} -> {playlist.Name} ({playlist.FileName})");

        var music = new List<MusicInfo>();
        GlobalDataBase.s_DbMusicTag.GetMusicInfosByUids(playlist.Resolve().ToList().ToIl2Cpp(), music);
        foreach (var info in music) __result.Add(info);

        // Playlists.Logger.Msg($"Playlist Count: {playlist.Albums.Count}; Resolved: {music.Count}");
    }
}