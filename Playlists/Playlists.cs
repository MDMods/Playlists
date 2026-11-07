using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Controls;
using MelonLoader;
using Playlists.Customs;
using Playlists.Interfaces;
using Playlists.Lists;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Playlists;

public class Playlists : MelonMod
{
    public static MelonLogger.Instance Logger { get; } = new(nameof(Playlists));
    public static bool CustomAlbumsInstalled { get; private set; }

    public static string PlaylistPath => Path.Combine(Directory.GetCurrentDirectory(), "UserData/Playlists");

    internal static bool FinishedLoading { get; set; }
    internal static Dictionary<IPlaylist, IEnumerable<string>> ResolveCache { get; } = new();

    public static List<IPlaylist> LoadedPlaylists => loadedPlaylists ??= getPlaylists();
    private static List<IPlaylist>? loadedPlaylists;

    private static readonly List<IPlaylist> registered_lists = new();

    public override void OnInitializeMelon()
    {
        CustomAlbumsInstalled = FindMelon("CustomAlbums", "Team Baller") is not null
                                || FindMelon("CustomAlbums", "Two Fellas") is not null;

        if (!CustomAlbumsInstalled) return;

        RegisterResolver(new CustomsResolver());
        RegisterPlaylist(new CustomsRankedPlaylist());
    }

    #region Resolvers

    private static readonly List<IAlbumResolver> resolvers = new();

    /// <summary>
    /// Registers a custom resolver for <see cref="ResolveReferences"/>.
    /// </summary>
    /// <param name="resolver"></param>
    public static void RegisterResolver(IAlbumResolver resolver) => resolvers.Add(resolver);

    /// <summary>
    /// Resolves references to any special album types like CustomAlbums (album_ prefix).
    /// </summary>
    public static IEnumerable<string> ResolveReferences(IEnumerable<string> input)
    {
        var result = new List<string>();

        foreach (var reference in input)
        {
            var match = resolvers.FirstOrDefault(x => x.ValidFor(reference));

            if (match is null)
            {
                result.Add(reference);
                continue;
            }

            var resolved = match.GetReference(reference);
            if (string.IsNullOrWhiteSpace(resolved)) continue;

            result.Add(resolved);
        }

        return result;
    }

    #endregion

    /// <summary>
    /// Registers a mod-provided playlist that will be added to the list when the main mod loads.
    /// </summary>
    public static void RegisterPlaylist(IPlaylist playlist) => registered_lists.Add(playlist);

    public static int ComparePlaylists(IPlaylist? one, IPlaylist? two)
    {
        if (ReferenceEquals(one, two))
            return 0;

        if (one is null || two is null)
            return 1;

        var result = one.Position.CompareTo(two.Position);
        return result != 0 ? result : string.Compare(one.Name, two.Name, StringComparison.Ordinal);
    }

    private static List<IPlaylist> getPlaylists()
    {
        ensureCreated();

        var files = Directory.GetFiles(PlaylistPath, "*.json");
        var list = files.Select(FileSystemPlaylist.ReadFromDisk).Cast<IPlaylist>().ToList();
        list.AddRange(registered_lists);
        list.Sort((a, b) => a.CompareTo(b));
        return list;
    }

    private static void ensureCreated()
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

    private bool pressed;

    private readonly KeyCode[] numKeys =
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

        var currentPressed = numKeys.FirstOrDefault(Input.GetKeyDown);
        var idx = Array.IndexOf(numKeys, currentPressed);

        if (idx >= 0)
        {
            if (pressed)
                return;

            pressed = true;

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
        else pressed = false;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (FinishedLoading)
            return;

        var wHalf = Screen.width / 2f;
        var rect = new Rect(wHalf - 100, Screen.height - 96, 200, 48);

        GUI.color = new Color(0.23529412f, 0.15686275f, 0.43529412f, 1f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(rect, "Loading Playlists...", new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        });
    }
}
