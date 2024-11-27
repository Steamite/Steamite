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
    /// Gets save name from path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetSaveName(string path)
    {
        string s = "";
        int i = path.Length;
        while (true)
        {
            i--;
            if (path[i] == '\\' || path[i] == '/')
                break;
            s += path[i];
        }
        return s.Reverse().ToArray().ArrayToString();
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
        JsonSerializer jsonSerializer = PrepSerializer();
        JsonWriter jsonWriter = null;
        try
        {
            SaveGrid(tmpPath, jsonSerializer);
            SaveHumans(tmpPath, jsonSerializer);
            SaveGameState(tmpPath, jsonSerializer);
            SaveResearch(tmpPath, jsonSerializer);
            SaveTrade(tmpPath, jsonSerializer);
        }
        catch (Exception e)
        {
            CanvasManager.ShowMessage("An error ocured when saving.");
            Debug.LogError("Saving error: " + e);
            if(jsonWriter.WriteState == 0)
            {
                jsonWriter.Close();
            }
            foreach (string file in Directory.GetFiles(tmpPath))
            {
                File.Delete(file);
            }
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
        if (Directory.GetDirectories($"{Application.persistentDataPath}/saves").FirstOrDefault(q => GetSaveName(q) == activeF) == null)
            Directory.CreateDirectory($"{path}");

        foreach (string file in Directory.GetFiles(tmpPath))
        {
            string s = file.Split("\\").Last();
            if (File.Exists($"{path}/{s}"))
                File.Delete($"{path}/{s}");
            File.Move($"{file}", $"{path}/{s}");
        }

        Directory.Delete($"{tmpPath}");
        CanvasManager.ShowMessage(autoSave ? "Autosave" : "Saved succesfuly");
    }


    //-------Grid-------\\
    void SaveGrid(string path, JsonSerializer jsonSerializer)
    {
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Grid.json"));
        BuildsAndChunksSave buildsAndChunksSave 
            = new(SaveBuildings(), SaveChunks());
        
        jsonSerializer.Serialize(jsonTextWriter, buildsAndChunksSave);
        jsonTextWriter.Close();
        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            jsonTextWriter = new(new StreamWriter($"{path}/Level{i}.json"));
            jsonSerializer.Serialize(jsonTextWriter, MyGrid.Save(i));
            jsonTextWriter.Close();
        }
    }
    BSave[] SaveBuildings()
    {
        BSave[] bSaves = new BSave[MyGrid.buildings.Count];
        for(int i = 0; i < MyGrid.buildings.Count; i++)
        {
            bSaves[i] = MyGrid.buildings[i].Save() as BSave;
        }
        return bSaves;
    }
    /*
    void SavePipes(WorldSave gridSave)
    {
        gridSave..pipes = new ClickableObjectSave[gridSave.height, gridSave.width];
        
        foreach(Pipe pipe in GameObject.Find("Pipes").GetComponentsInChildren<Pipe>())
        {
            GridPos pos = new(pipe.gameObject);
            gridSave.pipes[(int)pos.x, (int)pos.z] = pipe.Save();
        }
    }*/
    ChunkSave[] SaveChunks()
    {
        ChunkSave[] chunkSave = new ChunkSave[MyGrid.chunks.Count];
        for (int i = 0; i < MyGrid.chunks.Count; i++)
        {
            chunkSave[i] = MyGrid.chunks[i].Save() as ChunkSave;
        }
        return chunkSave;
    }
    //------Game State------\\
    void SaveGameState(string path, JsonSerializer jsonSerializer)
    {
        GameStateSave gameState = new();
        gameState.priorities = SceneRefs.humans.GetComponent<JobQueue>().priority;
        SceneRefs.tick.timeController.Save(gameState);

        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/PlayerSettings.json"));
        jsonSerializer.Serialize(jsonTextWriter, gameState);
        jsonTextWriter.Close();
    }

    //------Humans------\\
    void SaveHumans(string path, JsonSerializer jsonSerializer)
    {
        HumanSave[] humanSave = SceneRefs.humans.SaveHumans();
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Humans.json"));
        jsonSerializer.Serialize(jsonTextWriter, humanSave);
        jsonTextWriter.Close();
    }
    
    //------Research------\\
    void SaveResearch(string path, JsonSerializer jsonSerializer)
    {
        // takes nodes from all research buttons
        //ResearchUI research = CanvasManager.research;
        /*Transform categsTransform = research.categoriesTran;
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
        jsonTextWriter.Close();*/
    }

    //------Research------\\
    void SaveTrade(string path, JsonSerializer jsonSerializer)
    {
        /*Trade trade = CanvasManager.trade;
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Trade.json"));
        jsonSerializer.Serialize(jsonTextWriter, new TradeSave(trade));
        jsonTextWriter.Close();*/
    }
}

