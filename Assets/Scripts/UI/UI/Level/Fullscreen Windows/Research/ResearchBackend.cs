using TMPro;
using UnityEngine;

public class ResearchBackend : MonoBehaviour
{
    ResearchUI UI;
    public ResearchUIButton currentResearch;
    public StorageResource researchResourceInput;

    public void Init(ResearchUI _UI)
    {
        UI = _UI;
        SceneRefs.researchAdapter.Init(DoResearch, DisplayInfoWindowDetails);
    }

    ResearchDispayData DisplayInfoWindowDetails()
    {
        ResearchDispayData data = new();
        if (currentResearch)
        {
            data.name = currentResearch.name;
            data.progress = $"{(currentResearch.node.currentTime/(float)currentResearch.node.researchTime):0%}  %";
        }
        else
        {
            data.name = "None";
            data.progress = "0 %";
        }
        data.color = Random.ColorHSV();
        return data;
    }
    //Start researching a research
    public void StartResearch(ResearchUIButton button)
    {
        if (button == currentResearch)
            return;
        if (currentResearch == null)
        {
            currentResearch = button;
            currentResearch.StartAnim();
        }
        else if (button != currentResearch)
        {
            currentResearch.EndAnim(true);
            currentResearch = button;
            currentResearch.StartAnim();
            // confirmation popup
        }
        UI.StartCoroutine(UI.UpdateButtonFill());
        UI.openResearchAnimator.SetFloat("Speed", 0.5f);
        UI.openResearchAnimator.SetTrigger("selected");
    }

    public void FinishResearch()
    {
        currentResearch.Complete();
        currentResearch = null;
    }
    
    /// <summary>
    /// Called by every worker in a research building
    /// </summary>
    /// <param name="efficiecy">Ammount to add.</param>
    public void DoResearch(float efficiecy)
    {
        if (currentResearch)
        {
            currentResearch.node.currentTime += efficiecy * 1;
        }
    }
}
