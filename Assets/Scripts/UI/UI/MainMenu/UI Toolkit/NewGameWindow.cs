using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

using RadioGroups;

public class NewGameWindow : MonoBehaviour
{
    VisualElement document;

    Button startButton;
    WorldRadioGroup worlds;


    int selectedOption;


    string worldName;
    [CreateProperty]
    public string WorldName
    {
        get { return worldName; }
        set
        {
            worldName = value;
            UpdateButtonState();
        }
    }

    public void Init(VisualElement _root)
    {
        document = _root.Q<VisualElement>("New-Game-Menu");
        startButton = document.Q<Button>("Start");
        worlds = document.Q<WorldRadioGroup>("Worlds");
        startButton.RegisterCallback<ClickEvent>(StartGame);
        
        document.Q<TextField>("World-Name").dataSource = this;
        //document.Q<TextField>("World-Name").dataSourcePath = new PropertyPath("value");
        _root.Q<Button>("New-Game-Button").RegisterCallback<ClickEvent>((_) => document.style.display = DisplayStyle.Flex);
        _root.Q<Button>("New-Close-Button").RegisterCallback<ClickEvent>(ResetWindow);
        
        ResetWindow();
    }

    void ResetWindow(ClickEvent _ = null)
    {
        worlds.Init(
            (i) =>
            {
                selectedOption = i;
                UpdateButtonState();
            });
        selectedOption = -1;
        startButton.AddToClassList("disabled-button");
        startButton.RemoveFromClassList("enabled-button");
        WorldName = "";
        document.style.display = DisplayStyle.None;
    }

    void UpdateButtonState()
    {
        Debug.Log("updating");
        if (selectedOption > -1 && worldName.Trim().Length > 0)
        {
            startButton.AddToClassList("enabled-button");
            startButton.RemoveFromClassList("disabled-button");
        }
        else
        {
            startButton.AddToClassList("disabled-button");
            startButton.RemoveFromClassList("enabled-button");
        }

        if (selectedOption < 1) 
        {
            document.Q<Label>("PlaceHolder").style.display = DisplayStyle.Flex;
            document.Q<ScrollView>("MapParams").style.display = DisplayStyle.None;
        }
        else
        {
            document.Q<Label>("PlaceHolder").style.display = DisplayStyle.None;
            document.Q<ScrollView>("MapParams").style.display = DisplayStyle.Flex;
        }
    }

    void StartGame(ClickEvent _ = null)
    {
        if (selectedOption > -1 && worldName.Trim().Length > 0)
        {
            CreateWorld(false);
        }
    }
    public void CreateWorld(bool overwrite)
    {
        string[] dirs = Directory.GetDirectories(Application.persistentDataPath);
        if (!Directory.GetDirectories(Application.persistentDataPath).Contains(Application.persistentDataPath + "/saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }
        if (worldName != null)
        {
            List<string> dir = Directory.GetDirectories(Application.persistentDataPath + "/saves").ToList();
            if (overwrite || !dir.Contains(Application.persistentDataPath + "/saves/" + worldName))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/saves/" + worldName);
                if(selectedOption == 0)
                    GameObject.Find("Canvas").GetComponent<LoadingScreen>().NewGame(worldName);
                else
                {
                    GameObject.Find("Canvas").GetComponent<LoadingScreen>().NewGame(worldName, gameObject.GetComponent<MapGeneration>().Seed);
                }
            }
            else
            {
                transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("set Value");
        }
    }
}
