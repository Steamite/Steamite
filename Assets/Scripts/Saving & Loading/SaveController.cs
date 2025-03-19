using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using System.Collections;

/// <summary>Unities Saving.</summary>
public class SaveController : MonoBehaviour
{
    /// <summary>Action trigger after a succesful saving(to update ui).</summary>
    Action saveUIAction;
    /// <summary>Name of the world.</summary>
    string worldName;
    #region Init
    private void Start()
    {
        SceneRefs.tick.timeController.SubscribeToEvent(() => SaveGame("", true), DayTime.TimeEventType.Day);
        UIRefs.pauseMenu.Init((s) => SaveGame(s, false), ref saveUIAction);
        worldName = MyGrid.worldName;
    }

    void OnApplicationQuit()
    {
        SaveGame("", true);
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
    #endregion

    /// <summary>
    /// Gets save name from path.
    /// </summary>
    /// <param name="path">Path of the save folder.</param>
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
        return string.Join("",s.Reverse().ToArray());
    }

    /// <summary>
    /// Checks if there's a saves folder, then tries to save data to a temporaly directory.<br/>
    /// If it succedes calls AfterSave().
    /// </summary>
    /// <param name="autoSave"></param>
    public void SaveGame(string saveName, bool autoSave)
    {
        if (Directory.GetDirectories($"{Application.persistentDataPath}").FirstOrDefault(q => q == $"{Application.persistentDataPath}/saves") == null)
            Directory.CreateDirectory($"{Application.persistentDataPath}/saves");

        if (!autoSave &&
            Directory.GetDirectories($"{Application.persistentDataPath}/saves/{MyGrid.worldName}")
            .Select(q => GetSaveName(q)).Contains(saveName))
        {
            ConfirmWindow.window.Open(
                () =>
                {
                    Save(saveName, autoSave);
                },
                "Override save",
                $"Are you sure you want to override this save: <color=\"orange\">{saveName}?");
        }
        else
        {
            Save(saveName, autoSave);
        }
    }

    /// <summary>
    /// Saves the game state into a temp folder that is then moved elsewhere.
    /// </summary>
    /// <param name="saveName">Name of the new save.</param>
    /// <param name="autoSave">Is it an auto save.</param>
    void Save(string saveName, bool autoSave) 
    {
        string tmpPath = $"{Application.persistentDataPath}/saves/_tmp";
        Directory.CreateDirectory($"{tmpPath}");
        JsonSerializer jsonSerializer = PrepSerializer();
        JsonWriter jsonWriter = null;
        try
        {
            SaveGrid(tmpPath, jsonSerializer);
            SaveHumans(tmpPath, jsonSerializer);
            SaveGameState(tmpPath, autoSave, jsonSerializer);
            SaveResearch(tmpPath, jsonSerializer);
            SaveTrade(tmpPath, jsonSerializer);

            if (autoSave)
                saveName = "autosave";
            AfterSave(tmpPath, saveName, autoSave);
        }
        catch (Exception e)
        {
            SceneRefs.ShowMessage("An error ocured when saving.");
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
    }

    /// <summary>
    /// Moves saves from tmp folder to persistent folder.
    /// </summary>
    /// <param name="tmpPath">Tmp folder path.</param>
    /// <param name="autoSave">Save name.</param>
    void AfterSave(string tmpPath, string saveName, bool autoSave)
    {
        if (worldName == "")
            worldName = "noname";

        string path = $"{Application.persistentDataPath}/saves/{worldName}";
        if (Directory.GetDirectories($"{Application.persistentDataPath}/saves").FirstOrDefault(q => GetSaveName(q) == worldName) == null)
            Directory.CreateDirectory($"{path}");
        path = $"{path}/{saveName}";
        Directory.CreateDirectory(path);

        foreach (string saveFile in Directory.GetFiles(tmpPath))
        {
            string s = saveFile.Split("\\").Last();
            if (File.Exists($"{path}/{s}"))
                File.Delete($"{path}/{s}");
            File.Move($"{saveFile}", $"{path}/{s}");
        }

        Directory.Delete($"{tmpPath}");
        SceneRefs.ShowMessage(autoSave ? "Autosave" : "Saved succesfuly");
        saveUIAction();
    }


    #region Save Parts
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
    void SaveGameState(string path, bool autoSave, JsonSerializer jsonSerializer)
    {
        GameStateSave gameState = new();
        gameState.priorities = SceneRefs.jobQueue.priority;
        SceneRefs.tick.timeController.Save(gameState);
        gameState.autoSave = autoSave;

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
        ResearchUI research = UIRefs.research;
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

    //------Trade------\\
    void SaveTrade(string path, JsonSerializer jsonSerializer)
    {
        JsonTextWriter jsonTextWriter = new(new StreamWriter($"{path}/Trade.json"));
        jsonSerializer.Serialize(jsonTextWriter, new TradeSave(UIRefs.trading));
        jsonTextWriter.Close();
    }
    #endregion
}
