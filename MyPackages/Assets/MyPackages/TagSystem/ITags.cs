using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITags
{
    public List<TagScriptableObject> GetTags();

    public void SetTags(params TagScriptableObject[] tags);

    public void AddTag(TagScriptableObject tag);

    public void AddTags(TagScriptableObject[] tagsToAdd);

    public void RemoveTag(TagScriptableObject tag);

    public void RemoveAllTags();

    public bool PassTagFilterCheck(TagFilter tagFilter);
}
