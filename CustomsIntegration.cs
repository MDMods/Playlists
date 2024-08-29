using System;
using CustomAlbums.Managers;

namespace Playlists;

public static class CustomsIntegration
{
    public const string Fallback = "00-00";
    public const string AlbumPrefix = "album_";
    
    public static string GetIDForAlbum(string name)
    {
        try
        {
            // function won't call if this is false
            // but just in case, who knows
            if (!Playlists.CustomAlbumsInstalled)
                return Fallback;

            if (!AlbumManager.LoadedAlbums.TryGetValue(name, out var album))
                return Fallback;

            return album.Uid;
        }
        catch (Exception ex)
        {
            Playlists.Logger.Error($"Failed to resolve custom album '{name}'!", ex);
            return Fallback;
        }
    }

    public static string GetAlbumFromID(string uid)
    {
        try
        {
            if (!Playlists.CustomAlbumsInstalled)
                return Fallback;

            var album = AlbumManager.GetByUid(uid);

            if (album is null)
                return Fallback;

            return album.AlbumName;
        }
        catch (Exception ex)
        {
            Playlists.Logger.Error($"Failed to resolve custom album with uid {uid}!", ex);
            return Fallback;
        }
    }
}