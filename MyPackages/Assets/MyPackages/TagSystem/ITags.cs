using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITags
{
    public List<TagScriptableObject> GetTags();

    public void SetTags(params TagScriptableObject[] tags);
}
