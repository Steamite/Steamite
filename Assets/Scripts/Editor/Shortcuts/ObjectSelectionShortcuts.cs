using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSelectionShortcuts : MonoBehaviour
{

    [MenuItem("Custom Selection/ info window _1", priority = -1)]
    static public void SelectInfoWindow()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().infoWindow.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Selection/ research view _2", priority = -1)]
    static public void SelectResearch()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().research.gameObject;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("Custom Selection/ pause menu _3", priority = -1)]
    static public void SelectPauseMenu()
    {
        GameObject gameObject = GameObject.Find("UI canvas").GetComponent<CanvasManager>().pauseMenu.gameObject;
        Selection.activeGameObject = gameObject;
    }
}
