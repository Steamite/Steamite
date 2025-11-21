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
    //static readonly string dataPath = "Assets/Editor/EditorData.asset";
    static bool continueGame = false;

    /// <summary>
    /// Needs to be initialized to register the event.
    /// </summary>
    static SceneLoadingShortucts()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Custom Editors/Load/Continue _1")]
    static void LoadGame()
    {
        if (!EditorApplication.isPlaying)
        {
            continueGame = true;
            OnPlayModeStateChanged(PlayModeStateChange.ExitingEditMode);
        }
    }
/*
    [MenuItem("Custom Editors/Load/Toggle loading _1")]
    static void ToggleLoading()
    {
        if(data == null)
            data = AssetDatabase.LoadAssetAtPath<SceneLoadingData>(dataPath);
        data.load = !data.load;
        EditorUtility.SetDirty(data);
        EditorUtility.DisplayDialog("Load mode changed", $"Continue is set to: {data.load}", "ok");
    }*/

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
                if (continueGame)
                    GameObject.Find("Loader").GetComponent<SplashScreen>().loadAction = LoadActions.LoadGame;
                else
                    GameObject.Find("Loader").GetComponent<SplashScreen>().loadAction = LoadActions.NewGame;
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
        continueGame = false;
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