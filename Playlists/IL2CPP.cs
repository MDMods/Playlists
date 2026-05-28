using System.Collections.Generic;

namespace Playlists;

public static class IL2CPP
{
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2Cpp<T>(this List<T> list)
    {
        var il2Cpp = new Il2CppSystem.Collections.Generic.List<T>(list.Count);
        foreach (var item in list) il2Cpp.Add(item);
        return il2Cpp;
    }
}