
using UnityEngine.SceneManagement;

public static class SceneManagerService
{
    public static void LoadScene(SceneReferenceScriptableObject scene, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(scene.GetSceneName(), loadSceneMode);
    }

    public static void UnloadScene(SceneReferenceScriptableObject scene)
    {
        SceneManager.UnloadSceneAsync(scene.GetSceneName());
    }


}
