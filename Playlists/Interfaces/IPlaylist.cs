using System;
using System.Collections.Generic;

namespace Playlists.Interfaces;

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
    
    /// <summary>
    /// Called after every <see cref="Add"/> and <see cref="Remove"/>.
    /// </summary>
    void SaveToDisk();

    /// <summary>
    /// Resolves and custom references in the albums list.
    /// If you don't want to do anything with this, return <see cref="Playlists.ResolveReferences"/>.
    /// </summary>
    /// <returns>A list of MusicUids</returns>
    IEnumerable<string> Resolve();
}