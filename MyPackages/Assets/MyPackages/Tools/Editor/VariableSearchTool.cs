#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;

public class VariableSearchTool : EditorWindow
{
    private string searchTypeName;
    private static string[] favoriteTypes = new string[] { null };
    private static int selectedFavoriteIndex;

    private static List<GameObject> highlightedGameObjects = new();

    private const string FavouritePrefsKey = "VariableSearchTool_Favourites";

    private static bool isSubscribed;
    private static bool searchIsActive;

    [MenuItem("Tools/Hierarchy Variable Search")]
    public static void OpenWindow()
    {
        var window = GetWindow<VariableSearchTool>("Variable Search");
        window.Show();
    }

    [InitializeOnLoadMethod]
    public static void Intialize()
    {
        if (isSubscribed) return;

        EditorSceneManager.sceneOpened += OnSceneOpened;
        isSubscribed = true;

        EditorApplication.quitting += OnQuitUnity;
    }

    private void OnEnable()
    {
        //Load from EditorPrefs
        LoadFavourites();

        selectedFavoriteIndex = 0; // Default to the first item

        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
    }

    private void OnDisable()
    {
        ResetHierarchy();
    }


    private void OnGUI()
    {
        EditorGUILayout.LabelField("Search GameObjects by Variable Type", EditorStyles.boldLabel);

        // Dropdown for selecting from favorites
        selectedFavoriteIndex = EditorGUILayout.Popup("Favorites:", selectedFavoriteIndex, favoriteTypes);

        // Textfield for custom search
        searchTypeName = EditorGUILayout.TextField("Variable Name:", searchTypeName);

        if (GUILayout.Button("Search"))
        {
            TrySearch();
        }

        if (GUILayout.Button("Reset"))
        {
            ResetHierarchy();
        }

        EditorGUILayout.Space();

        // Allow adding a custom type to favorites
        if (GUILayout.Button("Add to Favorites"))
        {
            AddToFavorites(searchTypeName);
        }

        // Allow removing a custom type from favorites
        if (GUILayout.Button("Remove from Favorites"))
        {
            RemoveFromFavorites(searchTypeName);
        }
    }

    private void TrySearch()
    {
        // Use the selected favorite type or custom type name
        var typeToSearch = favoriteTypes[selectedFavoriteIndex];
        if (!string.IsNullOrEmpty(searchTypeName))
        {
            typeToSearch = searchTypeName;
        }

        var searchType = ResolveType(typeToSearch);

        if (searchType == null)
        {
            Debug.LogError($"Type '{typeToSearch}' not found. Ensure it's a valid C# or Unity type.");
            return;
        }

        SearchByType(searchType);
    }

    private void SearchByType(Type searchType)
    {
        var allObjects = GetAllObjects();

        highlightedGameObjects.Clear();

        foreach (var obj in allObjects)
        {
            bool hasVariable = HasVariableOfType(obj, searchType) || HasVariableInChildren(obj, searchType);

            if (hasVariable)
            {
                if (HasVariableOfType(obj, searchType)) highlightedGameObjects.Add(obj);
                obj.hideFlags = HideFlags.None;
            }
            else
            {
                obj.hideFlags = HideFlags.HideInHierarchy;
            }
        }
       

        searchIsActive = true;

        Debug.Log($"Filtered hierarchy by type: {searchType}");
    }

    private bool HasVariableInChildren(GameObject parent, Type searchType)
    {
        foreach(Transform child in parent.transform)
        {
            if(HasVariableOfType(child.gameObject, searchType) || HasVariableInChildren(child.gameObject, searchType))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasVariableOfType(GameObject obj, Type searchType)
    {
        var components = obj.GetComponents<MonoBehaviour>();

        foreach (var component in components)
        {
            if (component == null) continue; // Skip missing scripts

            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fields.Any(field => field.FieldType == searchType))
                return true;
        }

        return false;
    }

    private void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if(obj == null) return;

        if (highlightedGameObjects.Contains(obj))
        {
            EditorGUI.DrawRect(selectionRect, new Color(1f, 1f, 0.5f, 0.3f));
        }
    }

    private static void ResetHierarchy()
    {
        if (!searchIsActive) return;

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            // We're in prefab mode, reset all objects in the prefab
            List<GameObject> prefabObjects = new();
            CollectAllChildren(prefabStage.prefabContentsRoot, prefabObjects);
            foreach (var obj in prefabObjects)
            {
                obj.hideFlags = HideFlags.None;
            }
        }
        else
        {
            // We're in scene mode
            var allObjects = FindObjectsOfType<GameObject>(true);
            foreach (var obj in allObjects)
            {
                obj.hideFlags = HideFlags.None;
            }
        }

        highlightedGameObjects.Clear();
        searchIsActive = false;

        Debug.Log("Reset hierarchy visibility.");
    }

    private GameObject[] GetAllObjects()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

        if (prefabStage != null)
        {
            return GetPrefabObjects(prefabStage);
        }

        return FindObjectsOfType<GameObject>(true);
    }

    private GameObject[] GetPrefabObjects(PrefabStage prefabStage)
    {
        List<GameObject> objects = new();
        CollectAllChildren(prefabStage.prefabContentsRoot, objects);
        return objects.ToArray();
    }

    private static void CollectAllChildren(GameObject parent, List<GameObject> objects)
    {
        if (parent == null) return;

        // Add the current object to the list
        objects.Add(parent);

        // Recursively add all children
        foreach (Transform child in parent.transform)
        {
            CollectAllChildren(child.gameObject, objects);
        }
    }

    private Type ResolveType(string typeName)
    {
        // Handle common C# and Unity types
        switch (typeName.ToLower())
        {
            case "int": return typeof(int);
            case "float": return typeof(float);
            case "bool": return typeof(bool);
            case "string": return typeof(string);
            case "double": return typeof(double);
            case "vector2": return typeof(Vector2);
            case "vector3": return typeof(Vector3);
            case "quaternion": return typeof(Quaternion);
            case "transform": return typeof(Transform);
        }

        // Try to resolve other Unity/C# types from assemblies
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
    }

    private void AddToFavorites(string typeToAdd)
    {
        if (favoriteTypes.Contains(typeToAdd)) return; // Avoid duplicates
        Array.Resize(ref favoriteTypes, favoriteTypes.Length + 1);
        favoriteTypes[favoriteTypes.Length - 1] = typeToAdd;

        SaveFavourites();

        Debug.Log($"Added '{typeToAdd}' to favorites.");
    }

    private void RemoveFromFavorites(string typeToRemove)
    {
        var index = Array.IndexOf(favoriteTypes, typeToRemove);
        if (index < 0) return;

        // Remove the type and resize the array
        for (int i = index; i < favoriteTypes.Length - 1; i++)
        {
            favoriteTypes[i] = favoriteTypes[i + 1];
        }
        Array.Resize(ref favoriteTypes, favoriteTypes.Length - 1);

        SaveFavourites();

        Debug.Log($"Removed '{typeToRemove}' from favorites.");
    }

    private void LoadFavourites()
    {
        var savedFavourites = EditorPrefs.GetString(FavouritePrefsKey, string.Empty);

        if (!string.IsNullOrEmpty(savedFavourites))
        {
            favoriteTypes = savedFavourites.Split(',');
        }
    }

    private void SaveFavourites()
    {
        var favouriteTypesString = string.Join(",", favoriteTypes);

        EditorPrefs.SetString(FavouritePrefsKey, favouriteTypesString);
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        ResetHierarchy();
    }

    private static void OnQuitUnity()
    {
        if (!isSubscribed) return;

        EditorSceneManager.sceneOpened -= OnSceneOpened;
        isSubscribed = false;
    }
}
#endif
