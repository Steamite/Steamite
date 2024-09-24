using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneLoadingShortucts : MonoBehaviour
{
    static readonly string scenePath = "Assets/Scenes/";
    [MenuItem("Custom Editors/Load/Play %q", priority = -1)]
    static void Play()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorSceneManager.SaveOpenScenes();
            string activeSceneName = EditorSceneManager.GetActiveScene().name;
            if (activeSceneName != "Open Scene")
            {
                EditorSceneManager.OpenScene($"{scenePath}Open Scene.unity");
                EditorSceneManager.activeSceneChangedInEditMode += test;
            }
            GameObject.Find("Loader").GetComponent<FirstScene>().loadNewGame = true;
            EditorApplication.EnterPlaymode();
            File.WriteAllText($"{Application.persistentDataPath}/openScene.txt", activeSceneName);
        }
        else
        {
            EditorApplication.ExitPlaymode();
        }
    }

    private static void test(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        EditorSceneManager.activeSceneChangedInEditMode -= test;
        EditorSceneManager.OpenScene($"{scenePath}{File.ReadAllText($"{Application.persistentDataPath}/openScene.txt")}.unity");
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
}
