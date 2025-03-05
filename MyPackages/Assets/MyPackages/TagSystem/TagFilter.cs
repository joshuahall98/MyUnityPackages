using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class TagFilter
{
    [SerializeField] TagScriptableObject[] mustHaveAll;
    [SerializeField] TagScriptableObject[] mustHaveAny;
    [SerializeField] TagScriptableObject[] cannotHaveAny;

    public TagScriptableObject[] MustHaveAll => mustHaveAll;
    public TagScriptableObject[] MustHaveAny => mustHaveAny;
    public TagScriptableObject[] CannotHaveAny => cannotHaveAny;
}
