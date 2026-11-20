using UnityEditor;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
namespace Editor {
    [InitializeOnLoad]
    public static class StartScene
    {
        private const string START_SCENE = "Assets/Scenes/Menu.unity";

        static StartScene()
        {
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(START_SCENE);
        }
    }
}
#endif