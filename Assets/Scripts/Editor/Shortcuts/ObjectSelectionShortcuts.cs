using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Editor shorcuts for selecting the more common scene objects.</summary>
public class ObjectSelectionShortcuts
{
    [MenuItem("Custom Editors/Selection/info window", priority = -1)]
    static public void SelectInfoWindow()
    {
        if (EditorSceneManager.GetActiveScene().name != "Level")
            return;
        GameObject gameObject = SceneRefs.infoWindow.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/trade view ", priority = -1)]
    static public void SelectResearch()
    {
        if (EditorSceneManager.GetActiveScene().name != "Level")
            return;
        //GameObject gameObject = UIRefs.trade.window.transform.GetChild(3).GetChild(1).GetChild(1).gameObject;
        //Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/pause menu ", priority = -1)]
    static public void SelectPauseMenu()
    {
        if (EditorSceneManager.GetActiveScene().name != "Level")
            return;
        GameObject gameObject = UIRefs.pauseMenu.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/time", priority = -1)]
    static public void SelectTimeMenu()
    {
        if (EditorSceneManager.GetActiveScene().name != "Level")
            return;
        GameObject gameObject = SceneRefs.BottomBar.GetChild(0).gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/resources", priority = -1)]
    static public void SelectResourceMenu()
    {
        if (EditorSceneManager.GetActiveScene().name != "Level")
            return;
        GameObject gameObject = SceneRefs.BottomBar.GetChild(1).GetChild(0).gameObject;
        Selection.activeGameObject = gameObject;
    }
}