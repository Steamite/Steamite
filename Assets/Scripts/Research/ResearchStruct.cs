using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ResearchStructs : MonoBehaviour
{
    //Variables
    // General
    public int id;
    public Button button; //The transform of the button that starts the research
    public string researchName;
    public ResearchUI UI;
    public TMP_Text text; //Text of the button
    private ResearchStructSaved save; //Save object - helper object for saving
    private string saveJson; //Save object string
    //Pre-research
    private int researchNeeded; //How many research points are needed to complete the research
    private Resource researchCost; //Cost of the research (Resources needed to start the research)
    //In progress
    public bool beingResearched; //Is the research currently being researched
    private int researchProgress; //How many research points have been gathered
    //Post-research
    public bool completed; //Is the research completed
    public Action unlocks; //List of buildings that are unlocked by this research
        

    //Methods
    private void CompleteResearch()
    {
        if (completed) return;
        completed = true;
        unlocks?.Invoke();
        //UI.ResearchFinishedPopUP(); does nothing yet
    }
    
    //Check if the research is completed
    private void CheckCompleted()
    {
        if (researchProgress>=researchNeeded) CompleteResearch();
    }

    //Add research points
    public void AddResearchPoints(int number, bool add = true, bool check = false, bool update = true)
    {
        if (add) researchProgress += number;
        else researchProgress -= number;
        if (check) CheckCompleted();
        if (update) UI.UpdateCounter(this, completed: completed);
    }
    
    //Save the research
    public ResearchStructSaved SaveBuilder(ResearchStructs saved)
    {
        save = new ResearchStructSaved();
        save.id = saved.id;
        save.researchName = saved.researchName;
        save.researchNeeded = saved.researchNeeded;
        save.researchCost = saved.researchCost;
        save.beingResearched = saved.beingResearched;
        save.researchProgress = saved.researchProgress;
        save.completed = saved.completed;
        return save;
    }
    
    //Load the research
    public void SaveLoader(ResearchStructSaved saved)
    {
        id = saved.id;
        researchName = saved.researchName;
        researchNeeded = saved.researchNeeded;
        researchCost = saved.researchCost;
        beingResearched = saved.beingResearched;
        researchProgress = saved.researchProgress;
        completed = saved.completed;
        text = null;
        button = null;
    }
    
    //Start research
    public void StartResearch(Button button1)
    {
        if (beingResearched) return;
        if (completed) return;
        button = button1;
        beingResearched = true;
        if(researchNeeded <= researchProgress)
        {
            CompleteResearch();
        }
        UI.UpdateCounter(this);
    }

    //Getters
    //Get research needed
    public int GetResearchNeeded()
    {
        return researchNeeded;
    }
    
    //Get research progress
    public int GetResearchProgress()
    {
        return researchProgress;
    }
    
    //Start & Awake
    //TODO: Get rid of this
    public void Awake()
    {
        //MainBackend = gameObject.GetComponent<ResearchBackend>();
        //UI = gameObject.GetComponent<ResearchUI>();
    }
}

//Research save object - helper class for saving
[Serializable]
public class ResearchStructSaved
{
    //Variables
        // General
            public int id;
            public string researchName;
        //Pre-research
            public int researchNeeded;
            public Resource researchCost;
        //In progress
            public bool beingResearched;
            public int researchProgress;
        //Post-research
            public bool completed;
}