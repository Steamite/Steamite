using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class LoadMenu : MonoBehaviour
{
    [SerializeField] GameObject content;
    [SerializeField] GameObject itemPrefab;
    //public event Action<GridSave, PlayerSettings, List<HumanSave>, string> loadGame;
    string selectedSave;
    List<string> loadedElems = new();

    WorldSave worldSave;
    GameStateSave gameStateSave;
    HumanSave[] humanSaves;
    ResearchSave researchSave;
    TradeSave tradeSave;

    public void ParseSaves()
    {
        string[] dirs = Directory.GetDirectories(Application.persistentDataPath + "/saves/");
        foreach (string path in dirs)
        {
            if (loadedElems.Contains(path))
            {
                continue;
            }
            TMP_Text item = Instantiate(itemPrefab, content.transform).transform.GetChild(0).GetComponent<TMP_Text>();
            item.text = GetSaveName(path);
            loadedElems.Add(path);

            Button b = item.transform.parent.GetComponent<Button>();
            b.onClick.AddListener(delegate { SelectSave(b); });
        }
        gameObject.SetActive(true);
    }

    public void ClearSelection(bool active)
    {
        selectedSave = "";
        transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "Info";
        transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = "";
        transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = false;
        transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = false;
        gameObject.SetActive(active);
    }

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

    void SelectSave(Button button)
    {
        selectedSave = button.transform.GetChild(0).GetComponent<TMP_Text>().text;
        JsonSerializer jsonSerializer = SaveController.PrepSerializer();
        // for gridSave
        worldSave = new();
        JsonTextReader jsonReader = new(new StreamReader($"{Application.persistentDataPath}/saves/{selectedSave}/Grid.json"));
        worldSave.objectsSave = jsonSerializer.Deserialize<BuildsAndChunksSave>(jsonReader);
        jsonReader.Close();

        worldSave.gridSave = new GridSave[MyGrid.NUMBER_OF_LEVELS];
        for(int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            jsonReader = new(new StreamReader($"{Application.persistentDataPath}/saves/{selectedSave}/Level{i}.json"));
            worldSave.gridSave[i] = jsonSerializer.Deserialize<GridSave>(jsonReader);
            jsonReader.Close();
        }

        // for playerSettings
        jsonReader = new(new StreamReader($"{Application.persistentDataPath}/saves/{selectedSave}/PlayerSettings.json"));
        gameStateSave = jsonSerializer.Deserialize<GameStateSave>(jsonReader);
        jsonReader.Close();
        // for humanSaves
        jsonReader = new(new StreamReader($"{Application.persistentDataPath}/saves/{selectedSave}/Humans.json"));
        humanSaves = jsonSerializer.Deserialize<HumanSave[]>(jsonReader);
        jsonReader.Close();
        // for researchCategories
        jsonReader = new(new StreamReader($"{Application.persistentDataPath}/saves/{selectedSave}/Research.json"));
        researchSave = jsonSerializer.Deserialize<ResearchSave>(jsonReader);
        jsonReader.Close();
        jsonReader = new(new StreamReader($"{Application.persistentDataPath}/saves/{selectedSave}/Trade.json"));
        tradeSave = jsonSerializer.Deserialize<TradeSave>(jsonReader);
        jsonReader.Close();

        transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = selectedSave;
        transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = // to show that the save is really working
            $"Buildings: {worldSave.objectsSave.buildings.Length}\n" +
            $"Humans: {humanSaves.Length}\n" +
            $"Completed Researches: {researchSave.categories.SelectMany(q=> q.nodes).Count(q=> q.researched)}";
        
        transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = true; // load
        transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = true; // delete
    }

    public void DeleteSave(bool delete)
    {
        if (!delete)
        {
            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(4).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = $"Are you sure you want do delete save: {selectedSave}";
        }
        else
        {
            Directory.Delete(Application.persistentDataPath + "/saves/" + selectedSave, true);
            int x = content.transform.childCount;
            for (int i = 0; i < x; i++)
            {
                string s = content.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text;
                if(s == selectedSave)
                {
                    content.transform.GetChild(i).gameObject.SetActive(false);
                    Destroy(content.transform.GetChild(i).gameObject);
                    selectedSave = "";
                    x--;
                    break;
                }
            }
            transform.GetChild(4).gameObject.SetActive(false);
            bool remain = x > 0;
            ClearSelection(remain);
            if(GameObject.Find("Main Menu"))
                transform.parent.GetChild(1).GetChild(1).GetComponent<Button>().interactable = remain;
        }
    }

    public void Load()
    {
        if(GameObject.Find("Main Menu") != null)
        {
            Load(null);
        }
        else
        {
            AsyncOperation aO = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            aO.completed += UnLoad;
        }
    }

    public void UnLoad(AsyncOperation _)
    {
        Scene scene = SceneManager.GetActiveScene();
        AsyncOperation aO = SceneManager.UnloadSceneAsync(scene);
        aO.completed += Load;
    }

    public void Load(AsyncOperation _)
    {
        GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().
            StartLoading(worldSave, gameStateSave, humanSaves, researchSave, tradeSave, selectedSave);
    }
}
