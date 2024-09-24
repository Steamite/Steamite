using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] public Transform stats;
    [SerializeField] public Transform buildMenu;
    [SerializeField] public Transform miscellaneous;
    [SerializeField] public UIOverlay overlays;
    [SerializeField] public Transform research;
    [SerializeField] public Menu pauseMenu;

    [Header("Child objects")]
    [SerializeField] public InfoWindow InfoWindow;

    void InitCanvases()
    {
        gameObject.SetActive(true);
    }
}
