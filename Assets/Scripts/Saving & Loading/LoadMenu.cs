using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class LoadMenu : MonoBehaviour
{
    [SerializeField] GameObject content;
    [SerializeField] GameObject itemPrefab;
    public event Action<GridSave, GameStateSave, List<HumanSave>, string> loadGame;
    string selectedSave;
    List<string> loadedElems = new();

    WorldSave worldSave;
    GameStateSave gameStateSave;
    HumanSave[] humanSaves;
    ResearchSave researchSave;
    TradeSave tradeSave;

    public void ParseSaves()
    {
        string dir = Directory.GetDirectories(Application.persistentDataPath + "/saves/").FirstOrDefault(q => q.Contains(MyGrid.worldName));
        if (dir != null && dir.Length > 0)
        {
            string[] saveFolders = Directory.GetDirectories(dir);
            foreach(string folder in saveFolders)
            {
                if (loadedElems.Contains(folder))
                    continue;
                else if (Directory.GetFiles(folder).Length == 0)
                    continue;
                TMP_Text item = Instantiate(
                    itemPrefab, content.transform).transform.GetChild(0).GetComponent<TMP_Text>();
                item.text = SaveController.GetSaveName(folder);
                loadedElems.Add(folder);

                Button b = item.transform.parent.GetComponent<Button>();
                b.onClick.AddListener(delegate { SelectSave(item.text); });
            }
            gameObject.SetActive(true);
            return;
        }
        gameObject.SetActive(false);
    }

    public void ClearSelection(bool active)
    {
        selectedSave = "";
        transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = MyGrid.worldName;
        transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = "";
        transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = false;
        transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = false;
        gameObject.SetActive(active);
    }
    void SelectSave(string save)
    {
        

        transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = MyGrid.worldName + '\n' + save;
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

    /*public async void Load()
    {
        if (GameObject.Find("Main Menu") == null)
        {
            await SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            Scene scene = SceneManager.GetActiveScene();
            await SceneManager.UnloadSceneAsync(scene);
        }
        GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().
            StartLoading(worldSave, gameStateSave, humanSaves, researchSave, tradeSave, selectedSave);
    }
*/
}
