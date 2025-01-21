using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class TagFilter
{
    [SerializeField] TagScriptableObject[] mustHaveAll;
    [SerializeField] TagScriptableObject[] mustHaveAny;
    [SerializeField] TagScriptableObject[] cannotHaveAny;

    public bool PassTagFilterCheck(Transform obj)
    {
        if (mustHaveAll != null && !obj.HasAllTags(mustHaveAll)) return false;
        if (mustHaveAny != null && mustHaveAny.Length > 0 && !obj.HasAnyTag(mustHaveAny)) return false;
        if (cannotHaveAny != null && obj.HasAnyTag(cannotHaveAny)) return false;
        return true;
    }
}
