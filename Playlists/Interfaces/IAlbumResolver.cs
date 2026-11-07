namespace Playlists.Interfaces;

public interface IAlbumResolver
{
    bool ValidFor(string reference);
    string? GetReference(string reference);
}