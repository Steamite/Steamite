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
    static JsonSerializer jsonSerializer;

    void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame(bool autoSave = false)
    {
        // checks if there's a folder for saves
        if (Directory.GetDirectories($"{Application.persistentDataPath}").FirstOrDefault(q => q == $"{Application.persistentDataPath}/saves") == null)
            Directory.CreateDirectory($"{Application.persistentDataPath}/saves");
        
        string tmpPath = $"{Application.persistentDataPath}/saves/_tmp";
        Directory.CreateDirectory($"{tmpPath}");
        try
        {
            SaveGrid(tmpPath);
            SaveHumans(tmpPath);
            SavePlayerSettings(tmpPath);
            SaveResearch(tmpPath);
        }
        catch (Exception e)
        {
            MyGrid.canvasManager.ShowMessage("An error ocured when saving.");
            Debug.Log("Saving error: " + e);
            Directory.Delete($"{tmpPath}");
            return;
        }

        if (activeFolder == "")
            activeFolder = "noname";
        string activeF = autoSave ? activeFolder + " - autosave" : activeFolder;
        string path = $"{Application.persistentDataPath}/saves/{activeF}";
        if (Directory.GetDirectories($"{Application.persistentDataPath}/saves").FirstOrDefault(q => LoadMenu.GetSaveName(q) == activeF) == null)
            Directory.CreateDirectory($"{path}");

        foreach(string file in Directory.GetFiles(tmpPath))
        {
            string s = file.Split("\\").Last();
            if(File.Exists($"{path}/{s}"))
                File.Delete($"{path}/{s}");
            File.Move($"{file}", $"{path}/{s}");
        }

        Directory.Delete($"{tmpPath}");
        MyGrid.canvasManager.ShowMessage(autoSave? "Autosave" : "Saved succesfuly");

    }

    void SaveResearch(string path)
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
                for(int k = 0; k < levelTransform.childCount; k++)
                {
                    categsData.categories[i].nodes.Add(levelTransform.GetChild(k).GetComponent<ResearchUIButton>().node);
                }
            }
        }
        if(research.GetComponent<ResearchBackend>().currentResearch)
            categsData.currentResearch = research.GetComponent<ResearchBackend>().currentResearch.node.id;
        //write saves
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Research.json"));
        PrepSerializer().Serialize(jsonTextWriter, categsData);
        jsonTextWriter.Close();
    }

    void SaveGrid(string path)
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
            PrepSerializer().Serialize(jsonTextWriter, gridSave);
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

    void SavePlayerSettings(string path)
    {
        PlayerSettings settings = new();
        settings.priorities = MyGrid.sceneReferences.humans.GetComponent<JobQueue>().priority;
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/PlayerSettings.json"));
        PrepSerializer().Serialize(jsonTextWriter, settings);
        jsonTextWriter.Close();
    }
    void SaveHumans(string path)
    {
        List<HumanSave> humanSave = new();
        foreach (Human h in MyGrid.sceneReferences.humans.GetComponent<Humans>().humen)
            humanSave.Add(new(h));
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Humans.json"));
        PrepSerializer().Serialize(jsonTextWriter, humanSave);
        jsonTextWriter.Close();
    }

    /// <summary>
    /// returns 
    /// </summary>
    /// <returns></returns>
    public static JsonSerializer PrepSerializer()
    {
        if(jsonSerializer == null)
        {
            jsonSerializer = new();
            jsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
            jsonSerializer.Formatting = Formatting.Indented;
        }
        return jsonSerializer;
    }
}

