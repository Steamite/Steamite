using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;


public class ResearchBackend : MonoBehaviour
{
    //Variables
    public ResearchStructs[] researches; //Researches
    private string saveFileLocation;
    private string defaultResearchLocation;
    public ResearchUI UI;
    public Transform researchesParent;

    //public Tick tick_script;

    private string saveJson; //Save file string
    private string defaultJson;

    //Temp
    private ResearchStructs research;
    
    public int numberOfResearches; //How many humans are employed in research buildings

    private int currentlyResearching; //Currently researched research
    [SerializeField]
    private List<ResearchStructSaved> savedResearches;
    private bool researching; //Is a research being done


    //Methods
    //Adds researchers - people who are currently in research buildings
    public void AddResearchers(int number, bool add = true)
    {
        if (add) numberOfResearches += number;
        else numberOfResearches -= number;
    }
    
    //Called by every worker in a research building
    public void DoResearch()
    {
        researches[currentlyResearching].AddResearchPoints(50, check: true);
    }
    
    //Start UI
    public void InitializeUI()
    {
        UI = gameObject.GetComponent<ResearchUI>();
        UI.Initialise();
    }

    //Start researching a research
    public void StartResearch(int id, Button button)
    {
        if(researching)
            if (id == currentlyResearching) return;
        currentlyResearching = id;
        researches[id].StartResearch(button);
        if (!researching)
        {
            researching = true;
            //tick_script.tickAction += DoResearch;
        }
    }
    
    public void InitializeResearchBackend()
    {
        //tick_script = GameObject.Find("Tick").GetComponent<Tick>();
    }

    public void PreInitializeResearches(int capacity, List<ResearchStructSaved> savedResearchStructsArray)
    {
        // TODO
        /*researches = new ResearchStructs[capacity];
        int counter = 0;
        foreach (ResearchStructSaved saved in savedResearchStructsArray)
        {
            research = researchesParent.gameObject.AddComponent<ResearchStructs>();
            research.SaveLoader(saved);
            researches[counter] = research;
            counter++;
        }
        foreach (ResearchStructs research in researches)
        {
            research.UI = UI;
        }*/
    }

    //Start & Awake
    
    //Find save file location
    /*
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            saveFileLocation = Application.persistentDataPath + "\\saves\\Researches.json";
        }
        else
        {
            saveFileLocation = Application.persistentDataPath + "/saves/Researches.json";
        }
    }*/
    private void Start()
    {
    }
}
