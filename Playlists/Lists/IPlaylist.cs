using System;
using System.Collections.Generic;

namespace Playlists.Lists;

public interface IPlaylist : IComparable<IPlaylist>
{
    string ID { get; }
    DateTime Creation { get; }
    DateTime LastModified { get; }
    string Name { get; }
    string Icon { get; }
    int Position { get; }
    List<string> Albums { get; }

    bool Add(string album);
    bool Remove(string album);
    void SaveToDisk();

    /// <summary>
    /// Gets the items from <see cref="Albums"/> and resolves any customs references.
    /// (Ignores album_ prefixes if customs isn't loaded.)
    /// </summary>
    /// <returns>A list of MusicUids</returns>
    IEnumerable<string> Resolve();
}