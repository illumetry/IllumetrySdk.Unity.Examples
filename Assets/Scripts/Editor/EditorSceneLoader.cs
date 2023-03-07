using UnityEditor;
using UnityEditor.SceneManagement;

public static class EditorSceneLoader
{
    [MenuItem("Scenes/Sandbox")]
    public static void LoadSandboxScene() {

        if (!EditorApplication.isPlaying) {
            EditorSceneManager.OpenScene("Assets/Scenes/Sandbox.unity");
        }
        else {
            EditorSceneManager.LoadScene("Sandbox");
        }
    }
}
