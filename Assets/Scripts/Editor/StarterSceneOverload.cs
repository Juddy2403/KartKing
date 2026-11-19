using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor {
    [InitializeOnLoad]
    public static class StartScene
    {
        private const string START_SCENE = "Assets/Scenes/PlayerLobby.unity";

        static StartScene()
        {
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(START_SCENE);
        }
    }
}