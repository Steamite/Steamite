using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SceneLoadingShortucts : MonoBehaviour
{
    static readonly string scenePath = "Assets/Scenes/";

    static SceneLoadingShortucts()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        return;/*
        if (state == PlayModeStateChange.ExitingEditMode)
        {

            EditorSceneManager.SaveOpenScenes();
            string activeSceneName = EditorSceneManager.GetActiveScene().name;
            if (activeSceneName != "Open Scene")
            {
                EditorSceneManager.OpenScene($"{scenePath}Open Scene.unity");
                //EditorSceneManager.activeSceneChangedInEditMode += SceneReturn;
            }
            GameObject.Find("Loader").GetComponent<FirstScene>().loadNewGame = true;
            EditorApplication.EnterPlaymode();
            File.WriteAllText($"{Application.persistentDataPath}/openScene.txt", activeSceneName);
        }
        else if(state == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.OpenScene($"{scenePath}{File.ReadAllText($"{Application.persistentDataPath}/openScene.txt")}.unity");
        }*/
    }


    [MenuItem("Custom Editors/Load/Open Scene _F1", priority = 0)]
    static void LoadOpenScene()
    {
        if (EditorSceneManager.GetActiveScene().name != "Open Scene")
            EditorSceneManager.OpenScene($"{scenePath}Open Scene.unity");
    }
    [MenuItem("Custom Editors/Load/Main Menu _F2", priority = 1)]
    static void LoadMainMenu()
    {
        if (EditorSceneManager.GetActiveScene().name != "Main Menu")
            EditorSceneManager.OpenScene($"{scenePath}Main Menu.unity");
    }
    [MenuItem("Custom Editors/Load/Level _F3", priority = 2)]
    static void LoadLevel()
    {
        if(EditorSceneManager.GetActiveScene().name != "Level")
            EditorSceneManager.OpenScene($"{scenePath}Level.unity");
    }
    [MenuItem("Custom Editors/Load/LoadingScreen _F4", priority = 3)]
    static void LoadLoadingScreen()
    {
        if (EditorSceneManager.GetActiveScene().name != "LoadingScreen")
            EditorSceneManager.OpenScene($"{scenePath}LoadingScreen.unity");
    }
}
