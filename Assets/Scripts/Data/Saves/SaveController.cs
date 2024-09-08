using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;

public class SaveController : MonoBehaviour
{
    public string activeFolder;

    static string Slash()
    {
        return "/";
        /*
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return "\\";
        }
        else
        {
            return "/";
        }*/
    }

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

        SaveGrid();
        SaveHumans();
        SavePlayerSettings();
        SaveResearch();
    }

    void SaveResearch()
    {
        ResearchSaveHandler research = GameObject.Find("Scene").GetComponent<SceneReferences>().research.GetComponent<ResearchSaveHandler>();
        research.SaveResearches($"{Application.persistentDataPath}{Slash()}saves{Slash()}{activeFolder}{Slash()}Research.json");
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
        settings.priorities = GameObject.Find("Humans").GetComponent<JobQueue>().priority;
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{Application.persistentDataPath}/saves/{activeFolder}/PlayerSettings.json"));
        PrepSerializer().Serialize(jsonTextWriter, settings);
        jsonTextWriter.Close();
    }
    void SaveHumans()
    {
        List<HumanSave> humanSave = new();
        foreach (Human h in GameObject.Find("Humans").GetComponent<Humans>().humen)
            humanSave.Add(new(h));
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{Application.persistentDataPath}/saves/{activeFolder}/Humans.json"));
        PrepSerializer().Serialize(jsonTextWriter, humanSave);
        jsonTextWriter.Close();
    }

    JsonSerializer PrepSerializer()
    {
        JsonSerializer jsonSerializer = new();
        jsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
        jsonSerializer.Formatting = Formatting.Indented;
        return jsonSerializer;
    }
}

