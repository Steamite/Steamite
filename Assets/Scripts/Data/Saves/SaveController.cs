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

    public void SaveGame()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if (activeFolder == "")
            activeFolder = "new folder";
        if (Directory.GetDirectories($"{Application.persistentDataPath}/saves").FirstOrDefault(q => LoadMenu.GetSaveName(q) == activeFolder) == null)
            Directory.CreateDirectory($"{Application.persistentDataPath}/saves/{activeFolder}");

        try
        {
            SaveGrid();
            SaveHumans();
            SavePlayerSettings();
            SaveResearch();
        }
        catch (Exception e)
        {
            MyGrid.canvasManager.ShowMessage("An error ocured when saving.");
            Debug.Log("Saving error: " + e);
            return;
        }
        MyGrid.canvasManager.ShowMessage("Saved succesfuly");
    }

    void SaveResearch()
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
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{Application.persistentDataPath}/saves/{activeFolder}/Research.json"));
        PrepSerializer().Serialize(jsonTextWriter, categsData);
        jsonTextWriter.Close();
    }

    void SaveGrid()
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
            JsonTextWriter jsonTextWriter = new(new StreamWriter($"{Application.persistentDataPath}/saves/{activeFolder}/Grid.json"));
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

    void SavePlayerSettings()
    {
        PlayerSettings settings = new();
        settings.priorities = MyGrid.sceneReferences.humans.GetComponent<JobQueue>().priority;
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{Application.persistentDataPath}/saves/{activeFolder}/PlayerSettings.json"));
        PrepSerializer().Serialize(jsonTextWriter, settings);
        jsonTextWriter.Close();
    }
    void SaveHumans()
    {
        List<HumanSave> humanSave = new();
        foreach (Human h in MyGrid.sceneReferences.humans.GetComponent<Humans>().humen)
            humanSave.Add(new(h));
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{Application.persistentDataPath}/saves/{activeFolder}/Humans.json"));
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

