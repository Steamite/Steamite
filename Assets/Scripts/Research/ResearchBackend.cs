using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text;
using UnityEngine.UI;
using Newtonsoft.Json;

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

        public void InitializeResearches()
        {
            foreach (ResearchStructs research in researches)
            {
                research.Initialize();
            }
        }

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
    
    //Start Build Buttons
    public void InitializeBuildButtons() //Sorry, ale jinak to nejde kvuli tomu ze komponenty nejsou jeste aktivni :(
    {
        Transform buildButtons = transform.parent.GetChild(0).GetChild(1);
        for (int i = 0; i < buildButtons.childCount; i++)// for each building category
        {
            for (int n = 0; n < buildButtons.GetChild(i).GetChild(0).GetChild(0).childCount; n++) // for each building in category
            {
                buildButtons.GetChild(i).GetChild(0).GetChild(0).GetChild(n).GetComponent<BuildButtons>().Initialize(gameObject.GetComponent<ResearchBackend>()); //initialize the button
            }
        }
    }

    public void InitializeResearchButtons()
    {
        ResearchUiButton button;
        for (int i = 0; i < transform.GetChild(0).GetChild(2).childCount ; i++)
        {
            button = transform.GetChild(0).GetChild(2).GetChild(i).GetChild(0).GetComponent<ResearchUiButton>();
            button.Initialise(UI);
            if(UI == null) Debug.Log("UI is null in backend");
            Debug.Log("Button initialized");
        }
        //if(FindObjectsOfType<ResearchUiButton>().Length == 0) Debug.Log("No buttons found");
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
        researches = new ResearchStructs[capacity];
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
        }
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
