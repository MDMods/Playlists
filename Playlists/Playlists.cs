using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Controls;
using Il2CppAssets.Scripts.UI.Panels;
using MelonLoader;
using Playlists.Lists;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Playlists;

public class Playlists : MelonMod
{
    public static MelonLogger.Instance Logger { get; } = new(nameof(Playlists));
    public static bool CustomAlbumsInstalled { get; private set; }

    public static string PlaylistPath => Path.Combine(Directory.GetCurrentDirectory(), "UserData/Playlists");
    public static List<IPlaylist> LoadedPlaylists { get; private set; } = new();

    public override void OnLateInitializeMelon()
    {
        LoadedPlaylists = GetPlaylists();
        CustomAlbumsInstalled = FindMelon("CustomAlbums", "Team Baller") is not null
                                || FindMelon("CustomAlbums", "Two Fellas") is not null;
    }

    public static IEnumerable<string> ResolvePlaylistAlbums(IPlaylist playlist)
    {
        foreach (var alb in playlist.Albums)
        {
            if (!alb.StartsWith(CustomsIntegration.AlbumPrefix))
                yield return alb;

            if (!CustomAlbumsInstalled)
            {
                Logger.Error($"Failed to resolve custom album '{alb}' because CustomAlbums is missing!");
                continue;
            }

            var result = CustomsIntegration.GetIDForAlbum(alb);
            if (result == CustomsIntegration.Fallback) continue;

            yield return result;
        }
    }

    /// <summary>
    /// Loads playlists from the filesystem. (Any mods wanting to add custom playlists should Postfix this)
    /// </summary>
    private static List<IPlaylist> GetPlaylists()
    {
        EnsureCreated();

        var files = Directory.GetFiles(PlaylistPath, "*.json");
        var list = files.Select(FileSystemPlaylist.ReadFromDisk).Cast<IPlaylist>().ToList();
        list.Sort((a, b) => a.CompareTo(b));
        return list;
    }

    private static void EnsureCreated()
    {
        if (Directory.Exists(PlaylistPath) && Directory.GetFiles(PlaylistPath, "*.json").Any())
            return;

        Directory.CreateDirectory(PlaylistPath);

        var defaultPlaylist = new FileSystemPlaylist
        {
            ID = "default.json",
            Name = "Custom Playlist",
            Icon = "https://mdmc.moe/cdn/melon.png",
            Position = 1,
            Albums = new List<string> { "0-47", "58-0", "58-2" }
        };

        defaultPlaylist.SaveToDisk();
    }

    private bool _pressed;

    private readonly KeyCode[] _numKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0
    };

    public override void OnUpdate()
    {
        base.OnUpdate();

        // stops this from triggering when a textbox is focused
        if (EventSystem.current?.currentSelectedGameObject is not null)
            return;

        var currentPressed = _numKeys.FirstOrDefault(Input.GetKeyDown);
        var idx = Array.IndexOf(_numKeys, currentPressed);

        if (idx >= 0)
        {
            if (_pressed)
                return;

            _pressed = true;

            if (idx >= LoadedPlaylists.Count)
                return;

            var playlist = LoadedPlaylists[idx];
            var selected = GlobalDataBase.s_DbMusicTag.CurMusicInfo();

            if (selected is null)
                return;

            var uid = selected.uid;

            if (uid.StartsWith("999"))
            {
                if (!CustomAlbumsInstalled)
                {
                    // I don't know how you would trigger this
                    // but, you never know...
                    ShowText.ShowInfo("Custom Albums is not installed!");
                    return;
                }

                uid = CustomsIntegration.GetAlbumFromID(uid);
            }

            var remove = false;
            bool success;

            if (playlist.Albums.Contains(uid))
            {
                success = playlist.Remove(uid);
                remove = true;
            }
            else
                success = playlist.Add(uid);

            if (!success)
            {
                ShowText.ShowInfo("Failed to add to playlist!");
                return;
            }

            playlist.SaveToDisk();
            ShowText.ShowInfo($"{(remove ? "Removed from" : "Added to")} playlist '{playlist.Name}'!");
        }
        else _pressed = false;
    }
}