using UnityEditor;
using UnityEngine;

public class ObjectSelectionShortcuts : MonoBehaviour
{
    #if UNITY_EDITOR
    [MenuItem("Custom Editors/Selection/info window _1", priority = -1)]
    static public void SelectInfoWindow()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().infoWindow.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/ research view _2", priority = -1)]
    static public void SelectResearch()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().research.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/pause menu _3", priority = -1)]
    static public void SelectPauseMenu()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().pauseMenu.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/time _4", priority = -1)]
    static public void SelectTimeMenu()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().stats.GetChild(0).gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Editors/Selection/resources _5", priority = -1)]
    static public void SelectResourceMenu()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().stats.GetChild(1).gameObject;
        Selection.activeGameObject = gameObject;
    }
    #endif
}
