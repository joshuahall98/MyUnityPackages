using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("GameObject/Tags")]
public class Tags : MonoBehaviour, ITags
{
    [SerializeField] private List<TagScriptableObject> tags;

    private void Awake()
    {
        foreach (var tag in tags)
            gameObject.RegisterGameObjectWithTag(tag);
    }

    public List<TagScriptableObject> GetTags()
    {
        return tags;
    }

    public void SetTags(params TagScriptableObject[] tags)
    {
        this.tags = tags.ToList();
    }

    public void AddTag(TagScriptableObject tag)
    {
        if (tag == null)
        {
            Debug.LogError($"Attempted to add a null tag to {gameObject.name}.");
            return;
        }

        // Add to the local tags list in the ITagComponent
        if (!tags.Contains(tag))
        {
            tags.Add(tag);
            // Register the GameObject globally with the tag
            gameObject.RegisterGameObjectWithTag(tag);
        }


    }

    public void AddTags(TagScriptableObject[] tagsToAdd)
    {
        if (tagsToAdd == null)
        {
            Debug.LogError($"Attempted to add a null or empty collection of tags to {gameObject.name}.");
            return;
        }

        foreach (var tag in tagsToAdd)
        {
            if (tag == null)
            {
                Debug.LogError($"A null tag was encountered in the collection for {gameObject.name}. Skipping.");
                continue;
            }

            AddTag(tag); // Reuse the AddTag method for consistency
        }

    }

    public void RemoveTag(TagScriptableObject tag)
    {
        if (tag == null)
        {
            Debug.LogError($"Attempted to remove a null tag from {gameObject.name}.");
            return;
        }

        // Remove from the local tags list
        if (tags.Remove(tag))
        {
            // Unregister the GameObject globally from the tag
            gameObject.UnregisterGameObjectWithTag(tag);
        }
    }

    public void RemoveAllTags()
    {
        if (tags.Count == 0)
        {
            Debug.LogWarning($"No tags to remove from {gameObject.name}.");
            return;
        }

        foreach (var tag in tags.ToList()) // Create a copy to avoid modifying the collection during iteration
        {
            RemoveTag(tag); // Reuse the RemoveTag method
        }
    }

    public bool PassTagFilterCheck(TagFilter tagFilter)
    {
        var mustHaveAll = tagFilter.MustHaveAll;
        var mustHaveAny = tagFilter.MustHaveAny;
        var cannotHaveAny = tagFilter.CannotHaveAny;

        if (mustHaveAll != null && !transform.HasAllTags(mustHaveAll)) return false;
        if (mustHaveAny != null && mustHaveAny.Length > 0 && !transform.HasAnyTag(mustHaveAny)) return false;
        if (cannotHaveAny != null && transform.HasAnyTag(cannotHaveAny)) return false;
        return true;
    }
}
