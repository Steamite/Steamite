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

    // Research MainBackend calls this before the first frame
    public void Initialize(ResearchBackend research_script_passed)
    {
        /*research_script = research_script_passed;
        if (unlocked_by == -1) gameObject.GetComponent<Button>().interactable = true;
        else if(research_script.researches.Length > 0)
        {
            research_script.researches[unlocked_by].unlocks += ResearchUnlock;
            if (research_script.researches[unlocked_by].GetResearchProgress() >= research_script.researches[unlocked_by].GetResearchNeeded() || research_script.researches[unlocked_by].completed)
            {
                gameObject.GetComponent<Button>().interactable = true;
            }
            else
            {
                gameObject.GetComponent<Button>().interactable = false;
            }
        }*/
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Transform t = MyGrid.canvasManager.buildMenu;
        t.GetChild(2).GetComponent<PreBuildInfo>().DisplayInfo(buildPrefab, t.InverseTransformPoint(transform.GetComponent<RectTransform>().position));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MyGrid.canvasManager.buildMenu.GetChild(2).GetComponent<PreBuildInfo>().HideInfo();
    }
}
