using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>Unities Saving.</summary>
public class SaveController : MonoBehaviour, IAfterLoad
{
    /// <summary>Action trigger after a succesful saving(to update ui).</summary>
    Action saveUIAction;
    /// <summary>Name of the world.</summary>
    string worldName;

    #region Init
    public void Init()
    {
        SceneRefs.tick.SubscribeToEvent(() => SaveGame("", true), Tick.TimeEventType.Day);
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
        return string.Join("", s.Reverse().ToArray());
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
        try
        {
            WriteSave(
                $"{tmpPath}/Grid.json",
                jsonSerializer,
                new BuildsAndChunksSave(SaveBuildings(), SaveChunks()));

            for (int i = 0; i < 5; i++)
                WriteSave(
                    $"{tmpPath}/Level{i}.json",
                    jsonSerializer,
                    MyGrid.Save(i));

            WriteSave(
                $"{tmpPath}/Humans.json",
                jsonSerializer,
                SceneRefs.humans.SaveHumans());

            WriteSave(
               $"{tmpPath}/Game State.json",
               jsonSerializer,
               SaveGameState(autoSave));

            WriteSave(
               $"{tmpPath}/Research.json",
               jsonSerializer,
               new ResearchSave(UIRefs.research));

            WriteSave(
               $"{tmpPath}/Trade.json",
               jsonSerializer,
               new TradeSave(UIRefs.trading));

            if (autoSave)
                saveName = "autosave";
            AfterSave(tmpPath, saveName, autoSave);
        }
        catch (Exception e)
        {
            SceneRefs.ShowMessage("An error ocured when saving.");
            Debug.LogWarning("Saving error: " + e);
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
    BSave[] SaveBuildings()
    {
        BSave[] bSaves = new BSave[MyGrid.buildings.Count];
        for (int i = 0; i < MyGrid.buildings.Count; i++)
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
    GameStateSave SaveGameState(bool autoSave)
    {
        GameStateSave gameState = new();
        gameState.priorities = SceneRefs.jobQueue.priority;
        SceneRefs.tick.Save(gameState);
        gameState.autoSave = autoSave;
        return gameState;
    }

    void WriteSave(string path, JsonSerializer jsonSerializer, object data)
    {
        StreamWriter writer = null;
        JsonTextWriter jsonWriter = null;
        try
        {
            writer = new StreamWriter(path);
            jsonWriter = new(writer);
            jsonSerializer.Serialize(jsonWriter, data);
            jsonWriter.Close();
        }
        catch (Exception e)
        {
            if (writer != null)
                writer.Dispose();
            if (jsonWriter != null)
                jsonWriter.Close();
            throw e;            
        }
    }
    #endregion
}
