using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using TMPro;

public class SaveController : MonoBehaviour
{
    public string activeFolder;

    void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// Creates a JsonSerializer.
    /// </summary>
    /// <returns>Returns a standardized JsonSerializer</returns>
    public static JsonSerializer PrepSerializer()
    {
        JsonSerializer jsonSerializer = new();
        jsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
        jsonSerializer.Formatting = Formatting.Indented;
        return jsonSerializer;
    }

    /// <summary>
    /// Checks if there's a saves folder, then tries to save data to a temporaly directory.<br/>
    /// If it succedes calls AfterSave().
    /// </summary>
    /// <param name="autoSave"></param>
    public void SaveGame(bool autoSave = false)
    {
        if (Directory.GetDirectories($"{Application.persistentDataPath}").FirstOrDefault(q => q == $"{Application.persistentDataPath}/saves") == null)
            Directory.CreateDirectory($"{Application.persistentDataPath}/saves");
        
        string tmpPath = $"{Application.persistentDataPath}/saves/_tmp";
        Directory.CreateDirectory($"{tmpPath}");
        try
        {
            JsonSerializer jsonSerializer = PrepSerializer();
            SaveGrid(tmpPath, jsonSerializer);
            SaveHumans(tmpPath, jsonSerializer);
            SavePlayerSettings(tmpPath, jsonSerializer);
            SaveResearch(tmpPath, jsonSerializer);
            SaveTrade(tmpPath, jsonSerializer);
        }
        catch (Exception e)
        {
            MyGrid.canvasManager.ShowMessage("An error ocured when saving.");
            Debug.Log("Saving error: " + e);
            Directory.Delete($"{tmpPath}");
            return;
        }

        AfterSave(tmpPath, autoSave);
    }

    /// <summary>
    /// Moves saves from tmp folder to persistent folder.
    /// </summary>
    /// <param name="tmpPath">Tmp folder path.</param>
    /// <param name="autoSave">Save name.</param>
    void AfterSave(string tmpPath, bool autoSave)
    {
        if (activeFolder == "")
            activeFolder = "noname";

        string activeF = autoSave 
            ? (activeFolder.Contains(" - autosave") 
                ? activeFolder 
                : (activeFolder + " - autosave"))
            : activeFolder;

        string path = $"{Application.persistentDataPath}/saves/{activeF}";
        if (Directory.GetDirectories($"{Application.persistentDataPath}/saves").FirstOrDefault(q => LoadMenu.GetSaveName(q) == activeF) == null)
            Directory.CreateDirectory($"{path}");

        foreach (string file in Directory.GetFiles(tmpPath))
        {
            string s = file.Split("\\").Last();
            if (File.Exists($"{path}/{s}"))
                File.Delete($"{path}/{s}");
            File.Move($"{file}", $"{path}/{s}");
        }

        Directory.Delete($"{tmpPath}");
        MyGrid.canvasManager.ShowMessage(autoSave ? "Autosave" : "Saved succesfuly");
    }


    //-------Grid-------\\
    void SaveGrid(string path, JsonSerializer jsonSerializer)
    {
        GridSave gridSave = new();
        gridSave.width = MyGrid.width;
        gridSave.height = MyGrid.height;
        gridSave.gridItems = new ClickableObjectSave[gridSave.width, gridSave.height];
        if (true)//MyGrid.grid != null)
        {
            for (int x = 0; x < MyGrid.height; x++)
            {
                for (int z = 0; z < MyGrid.width; z++)
                {
                    ClickableObject clickable = MyGrid.GetGridItem(new(x, z));
                    if (clickable.GetType() != typeof(Building))
                        gridSave.gridItems[x, z] = clickable.Save();
                }
            }
            SaveBuildings(gridSave);
            SaveChunks(gridSave);
            JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Grid.json"));
            jsonSerializer.Serialize(jsonTextWriter, gridSave);
            jsonTextWriter.Close();
        }
    }
    void SaveBuildings(GridSave gridSave)
    {
        SavePipes(gridSave);
        gridSave.buildings = new();
        foreach (Building building in MyGrid.buildings)
        {
            BSave clickable = building.Save() as BSave;
            if (clickable != null)
                gridSave.buildings.Add(clickable);
        }
    }
    void SavePipes(GridSave gridSave)
    {
        gridSave.pipes = new ClickableObjectSave[gridSave.height, gridSave.width];
        foreach(Pipe pipe in GameObject.Find("Pipes").GetComponentsInChildren<Pipe>())
        {
            GridPos pos = new(pipe.gameObject);
            gridSave.pipes[(int)pos.x, (int)pos.z] = pipe.Save();
        }
    }
    void SaveChunks(GridSave gridSave)
    {
        gridSave.chunks = new();
        foreach(Chunk chunk in MyGrid.chunks)
        {
            gridSave.chunks.Add(chunk.Save() as StorageObjectSave);
        }
    }

    //------Player Settings------\\
    void SavePlayerSettings(string path, JsonSerializer jsonSerializer)
    {
        PlayerSettings settings = new();
        settings.priorities = MyGrid.sceneReferences.humans.GetComponent<JobQueue>().priority;
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/PlayerSettings.json"));
        jsonSerializer.Serialize(jsonTextWriter, settings);
        jsonTextWriter.Close();
    }

    //------Humans------\\
    void SaveHumans(string path, JsonSerializer jsonSerializer)
    {
        List<HumanSave> humanSave = new();
        foreach (Human h in MyGrid.sceneReferences.humans.GetComponent<Humans>().humen)
            humanSave.Add(new(h));
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Humans.json"));
        jsonSerializer.Serialize(jsonTextWriter, humanSave);
        jsonTextWriter.Close();
    }
    
    //------Research------\\
    void SaveResearch(string path, JsonSerializer jsonSerializer)
    {
        // takes nodes from all research buttons
        ResearchUI research = MyGrid.canvasManager.research;
        Transform categsTransform = research.categoriesTran;
        ResearchSave categsData = new(categsTransform.childCount);
        for (int i = 0; i < categsTransform.childCount; i++)
        {
            Transform categTransform = categsTransform.GetChild(i);
            categsData.categories[i] = new(categTransform.name);
            for (int j = 1; j < categTransform.childCount; j++)
            {
                Transform levelTransform = categTransform.GetChild(j);
                for (int k = 0; k < levelTransform.childCount; k++)
                {
                    categsData.categories[i].nodes.Add(levelTransform.GetChild(k).GetComponent<ResearchUIButton>().node);
                }
            }
        }
        if (research.GetComponent<ResearchBackend>().currentResearch)
            categsData.currentResearch = research.GetComponent<ResearchBackend>().currentResearch.node.id;
        //write saves
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Research.json"));
        jsonSerializer.Serialize(jsonTextWriter, categsData);
        jsonTextWriter.Close();
    }

    //------Research------\\
    void SaveTrade(string path, JsonSerializer jsonSerializer)
    {
        Trade trade = MyGrid.canvasManager.trade;
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Trade.json"));
        jsonSerializer.Serialize(jsonTextWriter, new TradeSave(trade));
        jsonTextWriter.Close();
    }
}

