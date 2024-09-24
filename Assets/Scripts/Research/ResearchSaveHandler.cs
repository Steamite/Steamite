using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

public class ResearchSaveHandler : MonoBehaviour
{
    //Variables
    private string saveJson;
    [SerializeField] private List<ResearchStructSaved> savedResearches;
    public ResearchBackend MainBackend;
    public ResearchUI UI;
    //Temp
    private ResearchStructs research;
    private int counter;
    private ResearchStructs[] researches;
    
    //Methods
    //Initializes all components
    private void InitializeEverything()
    {
        MainBackend.PreInitializeResearches(savedResearches.Count, savedResearches);
        MainBackend.InitializeUI();
        MainBackend.InitializeResearchBackend();
    }
    
    //Loads researches from file
    public void LoadResearches(string path)
    {
        if (!File.Exists(path))
        {
            File.Create(path);
            File.WriteAllText(path, "{}");
        }
        saveJson = File.ReadAllText(path);
        if (savedResearches == null) savedResearches = new List<ResearchStructSaved>();
        savedResearches.Clear();
        savedResearches = JsonConvert.DeserializeObject<List<ResearchStructSaved>>(saveJson);
        if (savedResearches == null)
        {
            savedResearches = new();
            Debug.LogWarning("No research data found in the JSON file.");
            return;
        }
        
        InitializeEverything();
    }
    
    //Initializes all researches
    private void InitializeResearches(int capacity)
    {
        researches = new ResearchStructs[capacity];
        counter = 0;
        foreach (ResearchStructSaved saved in savedResearches)
        {
            research = new ResearchStructs();
            research.SaveLoader(saved);
            researches[counter] = research;
            counter++;
        }
        MainBackend.researches = researches.ToArray();
        foreach (ResearchStructs research in researches)
        {
            research.UI = UI;
        }
        
        //MainBackend.researches = new ResearchStructs[capacity];}}
        //researches.CopyTo(MainBackend.researches, 0);
    }
    
    //Saves researches to file
    public void SaveResearches(string path)
    {
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            File.WriteAllText(path, "{}");
        }

        savedResearches.Clear();
        MainBackend.researches = MainBackend.researchesParent.GetComponentsInChildren<ResearchStructs>().ToList().ToArray();
        if(MainBackend.researches == null) Debug.Log("Researches are null");
        foreach (ResearchStructs research in MainBackend.researches)
        {
            savedResearches.Add(research.SaveBuilder(research));
        }

        saveJson = Newtonsoft.Json.JsonConvert.SerializeObject(savedResearches, Formatting.Indented);
        if (!string.IsNullOrEmpty(saveJson) && !saveJson.Equals("{}"))
        {
            File.WriteAllText(path, saveJson);
            Debug.Log("Saved research data to file."); // Debug log to indicate successful write
        }
        else
        {
            Debug.Log("JSON data is empty despite having research data.");
        }
    }
    
    //Creates new researches from default JSON file
    public void NewResearch()
    {

        /*if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) saveJson = Resources.Load<TextAsset>("Defaults/Researches_default").ToString();
        else saveJson = Resources.Load<TextAsset>("Defaults/Researches_default").ToString();
        */
        // this should be enough
        saveJson = Resources.Load<TextAsset>("Defaults/Researches_default").ToString();

        if (savedResearches == null) savedResearches = new List<ResearchStructSaved>();
        savedResearches.Clear();
        savedResearches = JsonConvert.DeserializeObject<List<ResearchStructSaved>>(saveJson);
        if (savedResearches == null)
        {
            Debug.Log("No research data found in the JSON file.");
            return;
        }
        
        InitializeEverything();
    }
}
