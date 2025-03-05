using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneReference", menuName = "ScriptableObject/SceneReference")]
public class SceneReferenceScriptableObject : ScriptableObject
{
    [SerializeField, HideInInspector] private string scenePath; // Runtime path for the scene

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset; // Editor-only reference



    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            // Save the scene's path for runtime use
            scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        }
    }
#endif

    public string GetSceneName()
    {
        if (string.IsNullOrEmpty(scenePath))
            return string.Empty;

        return System.IO.Path.GetFileNameWithoutExtension(scenePath);
    }
}
