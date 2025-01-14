using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneReference", menuName = "ScriptableObject/SceneReference")]
public class SceneReferenceScriptableObject : ScriptableObject
{
    [SerializeField] private SceneAsset sceneAsset; // Editor-only reference
    [SerializeField, HideInInspector] private string scenePath; // Runtime path for the scene

    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            // Save the scene's path for runtime use
            scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        }
    }

    public string GetSceneName()
    {
        if (string.IsNullOrEmpty(scenePath))
            return string.Empty;

        return System.IO.Path.GetFileNameWithoutExtension(scenePath);
    }
}
