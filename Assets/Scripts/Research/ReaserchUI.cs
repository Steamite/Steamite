using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchUI : MonoBehaviour
{
    //Variables
    private ResearchBackend Backend;
    private int selected_cattegory;
    private Transform[] cattegories;
    public TMP_Text counter;
    public Transform cattegories_parrent;
    
    //Methods
    //Initializes the UI
    public void Initialise()
    {
        Backend = gameObject.GetComponent<ResearchBackend>();
        selected_cattegory = 0;
        cattegories = new Transform[4];
        for(int i = 0; i < cattegories_parrent.childCount; i++)
        {
            cattegories[i] = (cattegories_parrent.GetChild(i));
        }
        transform.GetChild(0).gameObject.SetActive(false);
    }
    
    //pop-up after research is finished
    public void ResearchFinishedPopUP()
    {
        //TODO: Add pop-up
    }
    //Changes the cattegory
    public void ChangeCattegory(int id)
    {
        selected_cattegory = id;
        foreach (Transform cattegory in cattegories)
        {
            cattegory.gameObject.SetActive(false);
        }
        cattegories[id].gameObject.SetActive(true);
    }

    //Research button click - starts research
    public void OnResearchButtonClick(int id, Button button)
    {
        Backend.StartResearch(id, button);
    }
    
    //Updates the progress counter
    public void UpdateCounter(ResearchStructs research, bool completed = false)
    {
        if (completed)
        {
            counter.text = "Research Progress: Completed";
            return;
        }
        counter.text = ("Research Progress: " + ((int)((float)(research.GetResearchProgress()/research.GetResearchNeeded()))* 100) + "%");
    }
    
    //Opens the research UI
    public void OpenResearchUI()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
    
    //Closes the research UI
    public void CloseButton()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        
    }
}