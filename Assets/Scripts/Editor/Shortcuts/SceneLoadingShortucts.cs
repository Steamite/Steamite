using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Editor shorcuts for quickly switching levels(only in edit mode).</summary>
[InitializeOnLoad]
public class SceneLoadingShortucts : MonoBehaviour
{
    /// <summary>Base path to the scene folde</summary>
    static readonly string scenePath = "Assets/Scenes/";

    /// <summary>
    /// Needs to be initialized to register the event.
    /// </summary>
    static SceneLoadingShortucts()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>
    /// If the scene is played from "Level" scene, then initializes all previus parts.
    /// </summary>
    /// <param name="state">New state.</param>
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (EditorSceneManager.GetActiveScene().name == "Level")
            {
                EditorSceneManager.SaveOpenScenes();
                string activeSceneName = EditorSceneManager.GetActiveScene().name;
                if (activeSceneName != "Splash Screen")
                {
                    EditorSceneManager.OpenScene($"{scenePath}Splash Screen.unity");
                    //EditorSceneManager.activeSceneChangedInEditMode += SceneReturn;
                }
                GameObject.Find("Loader").GetComponent<SplashScreen>().loadNewGame = true;
                EditorApplication.EnterPlaymode();
                File.WriteAllText($"{Application.persistentDataPath}/openScene.txt", activeSceneName);
            }
            else
            {
                File.Delete($"{Application.persistentDataPath}/openScene.txt");
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (File.Exists($"{Application.persistentDataPath}/openScene.txt"))
                EditorSceneManager.OpenScene($"{scenePath}{File.ReadAllText($"{Application.persistentDataPath}/openScene.txt")}.unity");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }


    [MenuItem("Custom Editors/Load/Splash Screen _F1", priority = 0)]
    static void LoadOpenScene()
    {
        if (EditorSceneManager.GetActiveScene().name != "Splash Screen")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{scenePath}Splash Screen.unity");
        }
    }
    [MenuItem("Custom Editors/Load/Main Menu _F2", priority = 1)]
    static void LoadMainMenu()
    {
        if (EditorSceneManager.GetActiveScene().name != "Main Menu")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{scenePath}Main Menu.unity");
        }
    }
    [MenuItem("Custom Editors/Load/Level _F3", priority = 2)]
    static void LoadLevel()
    {
        if (EditorSceneManager.GetActiveScene().name != "Level")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{scenePath}Level.unity");
        }
    }
    [MenuItem("Custom Editors/Load/LoadingScreen _F4", priority = 3)]
    static void LoadLoadingScreen()
    {
        if (EditorSceneManager.GetActiveScene().name != "LoadingScreen")
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene($"{scenePath}LoadingScreen.unity");
        }
    }
}