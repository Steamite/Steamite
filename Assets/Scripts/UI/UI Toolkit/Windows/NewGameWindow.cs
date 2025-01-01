using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using RadioGroups;
using UnityEditor;

namespace StartMenu
{
    public class NewGameWindow : MonoBehaviour, IToolkitController, IGridMenu
    {
        VisualElement menu;

        Button startButton;
        WorldRadioGroup worlds;

        int selectedOption;
        string worldName;

        bool opened;

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
            menu = _root.Q<VisualElement>("New-Menu");
            startButton = menu.Q<Button>("Start");
            worlds = menu.Q<WorldRadioGroup>("Worlds");
            startButton.RegisterCallback<ClickEvent>(StartGame);

            menu.Q<TextField>("World-Name").dataSource = this;
            //document.Q<TextField>("World-Name").dataSourcePath = new PropertyPath("value");
            _root.Q<Button>("New-Game-Button").RegisterCallback<ClickEvent>(OpenWindow);
            _root.Q<Button>("New-Close-Button").RegisterCallback<ClickEvent>(CloseWindow);
            startButton.AddToClassList("disabled-button");
            startButton.RemoveFromClassList("main-button");
        }

        #region Window Logic
        public bool IsOpen()
        {
            return false;
        }
        public void OpenWindow(ClickEvent _ = null)
        {
            selectedOption = -1;
            WorldName = ""; 
            worlds.Init(
                (i) =>
                {
                    selectedOption = i;
                    UpdateButtonState();
                });
            gameObject.GetComponent<MyMainMenu>().OpenWindow("new");
        }

        public void CloseWindow(ClickEvent _ = null) => gameObject.GetComponent<MyMainMenu>().CloseWindow();

        public void UpdateButtonState()
        {
            Debug.Log("updating");
            if (selectedOption > -1 && worldName.Trim().Length > 0)
            {
                startButton.AddToClassList("main-button");
                startButton.RemoveFromClassList("disabled-button");
            }
            else
            {
                startButton.AddToClassList("disabled-button");
                startButton.RemoveFromClassList("main-button");
            }

            if (selectedOption < 1)
            {
                menu.Q<Label>("PlaceHolder").style.display = DisplayStyle.Flex;
                menu.Q<ScrollView>("MapParams").style.display = DisplayStyle.None;
            }
            else
            {
                menu.Q<Label>("PlaceHolder").style.display = DisplayStyle.None;
                menu.Q<ScrollView>("MapParams").style.display = DisplayStyle.Flex;
            }
        }

        #endregion

        #region Logic
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
                List<string> dir = Directory.GetDirectories(Application.persistentDataPath + "/saves").Select(q => SaveController.GetSaveName(q)).ToList();
                if (overwrite || !dir.Contains(worldName))
                {
                    if (overwrite)
                        Directory.Delete(Application.persistentDataPath + "/saves/" + worldName, true);
                    Directory.CreateDirectory(Application.persistentDataPath + "/saves/" + worldName);
                    if (selectedOption == 0)
                        GameObject.Find("Canvas").GetComponent<LoadingScreen>().NewGame(worldName);
                    else
                        GameObject.Find("Canvas").GetComponent<LoadingScreen>().NewGame(worldName, gameObject.GetComponent<MapGeneration>().Seed);
                }
                else
                {
                    transform.GetComponent<ConfirmWindow>().Open(
                        () => CreateWorld(true),
                        "Overwrite save",
                        $"Are you sure you want to\n" +
                        $"overwrite this save: <color=\"orange\">{worldName}</color>?");
                }
            }
            else
            {
                Debug.LogError("set Value");
            }
        }
        #endregion
    }
}