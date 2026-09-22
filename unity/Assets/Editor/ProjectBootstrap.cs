#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CryptoMining.Editor
{
    [InitializeOnLoad]
    public static class ProjectBootstrap
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        static ProjectBootstrap()
        {
            EditorApplication.delayCall += EnsureMainScene;
        }

        [MenuItem("Tools/Crypto Mining/Create Main Scene")]
        public static void EnsureMainScene()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            if (!File.Exists(ScenePath))
            {
                // The editor starts with an unsaved "Untitled" scene.
                // Creating another scene additively fails in that state, so create
                // the generated Main scene as the single active scene instead.
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "Main";
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
                Debug.Log("Crypto Mining: Assets/Scenes/Main.unity created.");
            }

            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            bool found = false;
            for (int i = 0; i < current.Length; i++)
                if (current[i].path == ScenePath) found = true;

            if (!found)
            {
                EditorBuildSettingsScene[] next = new EditorBuildSettingsScene[current.Length + 1];
                for (int i = 0; i < current.Length; i++) next[i] = current[i];
                next[next.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = next;
            }
        }

        [MenuItem("Tools/Crypto Mining/Open Main Scene")]
        public static void OpenMainScene()
        {
            EnsureMainScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
#endif
