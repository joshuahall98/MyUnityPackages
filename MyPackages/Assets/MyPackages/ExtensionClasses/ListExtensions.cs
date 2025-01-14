using System.Collections;
using System.Collections.Generic;
using System;

public static class ListExtensions
{
    public static T TryGet<T>(this List<T> list, int index)
    {
        return (index >= 0 && index < list.Count) ? list[index] : default;
    }
}
