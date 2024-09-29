using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ResearchBackend : MonoBehaviour
{
    ResearchUI UI;
    public ResearchUIButton currentResearch;
    public float speed = 3;
    public int queuedRoutines = 0;
    public float elapsedProgress;
    
    public void Init(ResearchUI _UI)
    {
        UI = _UI;
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
        elapsedProgress = currentResearch.node.currentTime;
        StartCoroutine(UpdateButtonFill());
    }

    void FinishResearch()
    {
        currentResearch.Complete();
        UI.ResearchFinishedPopUP();
        currentResearch = null;
    }
    
    //Called by every worker in a research building
    public void DoResearch()
    {
        if (currentResearch)
        {
            currentResearch.node.currentTime++;
            if(currentResearch.node.currentTime == currentResearch.node.researchTime)
            {
                FinishResearch();
            }
        }
    }

    public IEnumerator UpdateButtonFill()
    {
        elapsedProgress = currentResearch.node.currentTime;
        while (currentResearch)
        {
            elapsedProgress = Mathf.Lerp(elapsedProgress, currentResearch.node.currentTime, Time.deltaTime * speed);
            currentResearch.borderFill.fillAmount = elapsedProgress / currentResearch.node.researchTime;
            yield return null;
        }
    }
}
