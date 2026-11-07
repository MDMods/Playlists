using System;
using CustomAlbums.Managers;
using Playlists.Interfaces;

namespace Playlists;

public class CustomsResolver : IAlbumResolver
{
    public bool ValidFor(string reference) => reference.StartsWith(CustomsIntegration.ALBUM_PREFIX);

    public string? GetReference(string reference)
    {
        if (!Playlists.CustomAlbumsInstalled)
            return null;

        var result = CustomsIntegration.GetIDForAlbum(reference);
        return result == CustomsIntegration.FALLBACK ? null : result;
    }
}

public static class CustomsIntegration
{
    public const string FALLBACK = "00-00";
    public const string ALBUM_PREFIX = "album_";

    public static string GetIDForAlbum(string name)
    {
        try
        {
            // function won't call if this is false
            // but just in case, who knows
            if (!Playlists.CustomAlbumsInstalled)
                return FALLBACK;

            if (!AlbumManager.LoadedAlbums.TryGetValue(name, out var album))
                return FALLBACK;

            return album.Uid;
        }
        catch (Exception ex)
        {
            Playlists.Logger.Error($"Failed to resolve custom album '{name}'!", ex);
            return FALLBACK;
        }
    }

    public static string GetAlbumFromID(string uid)
    {
        try
        {
            if (!Playlists.CustomAlbumsInstalled)
                return FALLBACK;

            var album = AlbumManager.GetByUid(uid);

            if (album is null)
                return FALLBACK;

            return album.AlbumName;
        }
        catch (Exception ex)
        {
            Playlists.Logger.Error($"Failed to resolve custom album with uid {uid}!", ex);
            return FALLBACK;
        }
    }
}
