using Newtonsoft.Json;
using RadioGroups;
using StartMenu;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadGameMenu : MonoBehaviour, IToolkitController, IGridMenu
{
    VisualElement menu;
    SaveRadioGroup worldGroup;

    VisualElement saveElement;
    SaveRadioGroup saveGroup;
    Button loadButton;

    Button continueButton;
    Button loadMenuButton;

    int selectedWorld;
    int selectedSave;

    Folder[] worlds;
    Folder[] saves;

    bool isMainMenu;

    public void Init(VisualElement root)
    {
        isMainMenu = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).buildIndex == 2)
                isMainMenu = true;
        }
        menu = root.Q<VisualElement>("Load-Menu");
        menu.Q<VisualElement>("Background").ClearClassList();
        if (isMainMenu)
        {
            worldGroup = menu.Q<SaveRadioGroup>("World-List");

            worldGroup.deleteAction = (int i) =>
            {
                GetComponent<ConfirmWindow>().Open(
                            () => DeleteWorld(i),
                            "Delete world",
                            $"Are you sure you want to\n" +
                            $"delete this world <color=\"red\">{worlds[i]}</color> and saves that belong to it?");
            };
            worldGroup.Init(
            (i) =>
            {
                selectedWorld = i;
                UpdateGrids();
            });


            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", false, true);
            continueButton = root.Q<Button>("Continue-Button");
            if (worlds != null && worlds.Length > 0)
            {
                continueButton.RegisterCallback<ClickEvent>(Continue);
                ToggleStyleButton(continueButton, true);
            }
            selectedWorld = -1;
        }
        else
        {
            worlds = new Folder[] { new() };
            worlds[0].path = $"{Application.persistentDataPath}/saves/{MyGrid.worldName}";
            selectedWorld = 0;
            root.Q<Label>("Load-Header").text += " - " + MyGrid.worldName;
        }
        selectedSave = -1;

        #region Save List
        saveElement = menu.Q<VisualElement>("Saves");
        saveGroup = menu.Q<SaveRadioGroup>("Save-List");
        saveGroup.deleteAction = (int i) =>
        {
            ConfirmWindow.window.Open(
                        () => DeleteSave(i),
                        "Delete world",
                        $"Are you sure you want to\n" +
                        $"delete this save <color=\"red\">{saves[i]}</color>?");
        };
        saveGroup.Init(
            (i) =>
            {
                selectedSave = i;
                ToggleStyleButton(loadButton, selectedWorld > -1 && selectedSave > -1);
            });
        #endregion

        #region Load Game button
        loadButton = menu.Q<Button>("Load");
        loadButton.RegisterCallback<ClickEvent>(LoadGame);
        #endregion

        menu.Q<VisualElement>("Background").AddToClassList("menu-view");
        loadMenuButton = root.Q<Button>("Load-Game-Button");
        loadMenuButton.RegisterCallback<ClickEvent>(OpenWindow);

        menu.Q<Button>("Save-Close-Button").RegisterCallback<ClickEvent>(CloseWindow);
        
        UpdateGrids();
        ToggleStyleButton(loadMenuButton, worlds != null && worlds.Length > 0);
    }

    #region Window Logic
    public bool IsOpen()
    {
        if (isMainMenu)
        {
            return false;
        }
        return menu.style.display == DisplayStyle.Flex;
    }
    public void OpenWindow(ClickEvent _ = null)
    {
        if(isMainMenu)
            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", true, true);
        if (worlds != null && worlds.Length > 0)
        {
            if (isMainMenu)
            {
                selectedWorld = -1;
                selectedSave = -1;
                UpdateGrids();
                gameObject.GetComponent<MyMainMenu>().OpenWindow("load");

            }
            else
            {
                selectedSave = -1;
                UpdateGrids();
                menu.style.display = DisplayStyle.Flex;
            }
        }
        else
        {
            ToggleStyleButton(loadMenuButton, false);
        }
    }
    public void CloseWindow(ClickEvent _ = null) 
    {
        if (isMainMenu)
            gameObject.GetComponent<MyMainMenu>().CloseWindow();
        else
            menu.style.display = DisplayStyle.None;
    }
    public void UpdateButtonState()
    {
        bool t = true;
        if (worlds == null || worlds.Length == 0)
        {
            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", false, true);
            if (worlds.Length == 0)
                t = false;
        }
        ToggleStyleButton(loadMenuButton, t);
    }

    void ToggleStyleButton(Button button, bool activate)
    {
        if (activate)
        {
            button.RemoveFromClassList("disabled-button");
            button.AddToClassList("main-button");
        }
        else
        {
            button.AddToClassList("disabled-button");
            button.RemoveFromClassList("main-button");
        }
    }

    public void UpdateGrids()
    {
        if (selectedWorld != -1)
        {
            menu.Q<Label>("Hint").style.display = DisplayStyle.None;
            menu.Q<VisualElement>("List").style.display = DisplayStyle.Flex;
            saves = saveGroup.FillItemSource(worlds[selectedWorld].path, true, false);
            if (saves == null)
            {
                CloseWindow();
                ToggleStyleButton(loadMenuButton, false);
                return;
            }
            selectedSave = -1;
            ToggleStyleButton(loadButton, selectedWorld > -1 && selectedSave > -1);
        }
        else
        {
            menu.Q<Label>("Hint").style.display = DisplayStyle.Flex;
            menu.Q<VisualElement>("List").style.display = DisplayStyle.None;
        }
    }
    #endregion


    #region Loading Logic
    void Continue(ClickEvent _)
    {
        if(worlds.Length > 0)
        {
            selectedWorld = 0;
            saves = saveGroup.FillItemSource(worlds[selectedWorld].path, true, false);
            if (saves == null)
            {
                ToggleStyleButton(continueButton, false);
                return;
            }
            selectedSave = 0;
            Load();
        }
    }

    void LoadGame(ClickEvent _)
    {
        Load();
    }

    async void Load()
    {
        if (selectedSave == -1 || selectedSave > saves.Length)
            return;
        if (GameObject.Find("Main Menu") == null)
        {
            Scene scene = SceneManager.GetActiveScene();
            await SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive); // scene 1 is loading screen
            await SceneManager.UnloadSceneAsync(scene);
        }
        else
        {
        }
        GameObject.Find("Loading Screen").transform.GetChild(0)
            .GetComponent<LoadingScreen>().StartLoading(saves[selectedSave].path, worlds[selectedWorld].ToString());
    }
    #endregion

    #region Save deletion
    private void DeleteWorld(int worldIndex)
    {
        try
        {
            Directory.Delete(worlds[worldIndex].path, true);
            worlds = worlds.Where(q => q.path != worlds[worldIndex].path).ToArray();
            worldGroup.RemoveItem(worldIndex);
            selectedWorld = -1;
            selectedSave = -1;
            UpdateGrids();
            if (worlds.Length == 0)
            {
                if(isMainMenu)
                    ToggleStyleButton(continueButton, false);
                ToggleStyleButton(loadMenuButton, false);
                CloseWindow();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Cannot delete world:\n" + ex);
        }
    }
    private void DeleteSave(int saveIndex)
    {
        try
        {
            if (saves.Length > 1)
            {
                Directory.Delete(saves[saveIndex].path, true);
                saves = saves.Where(q => q.path != saves[saveIndex].path).ToArray();
                saveGroup.RemoveItem(saveIndex);
                selectedSave = -1;
                ToggleStyleButton(loadButton, selectedWorld > -1 && selectedSave > -1);
            }
            else
                DeleteWorld(selectedWorld);
        }
        catch(Exception ex)
        {
            Debug.LogError("Cannot delete save:\n" + ex);
        }
    }
    #endregion Save deletion
}
