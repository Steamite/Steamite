using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ResearchBackend research_script;
    public Building buildPrefab;
    public int unlocked_by; // The research that unlocks this building (-1 = unlocked on start)

    // selects tile to build
    public void SelPrefab()
    {
        GridTiles sel = GameObject.Find("Grid").GetComponent<GridTiles>();
        sel.buildingPrefab = buildPrefab;
        sel.ChangeSelMode(SelectionMode.build);
    }
    
    public void ResearchUnlock()
    {
        gameObject.GetComponent<Button>().interactable = true;
    }

    public void Initialize(Building prefab)
    {
        buildPrefab = prefab;
        name = prefab.name;
        transform.GetChild(0).GetComponent<TMP_Text>().text = name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Transform t = MyGrid.canvasManager.miscellaneous;
        t.GetChild(1).GetComponent<LocalInfoWindow>().DisplayInfo(buildPrefab, t.InverseTransformPoint(transform.GetComponent<RectTransform>().position));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MyGrid.canvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().HideInfo();
    }
}
