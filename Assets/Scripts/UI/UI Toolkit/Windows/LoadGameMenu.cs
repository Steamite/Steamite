using Newtonsoft.Json;
using RadioGroups;
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
    public void Init(VisualElement root)
    {
        menu = root.Q<VisualElement>("Load-Menu");
        worldGroup = menu.Q<SaveRadioGroup>("World-List");
        worldGroup.deleteAction = DeleteWorld;

        saveElement = menu.Q<VisualElement>("Saves");
        saveGroup = menu.Q<SaveRadioGroup>("Save-List");
        saveGroup.deleteAction = DeleteSave;

        loadButton = menu.Q<Button>("Load");
        loadButton.RegisterCallback<ClickEvent>(LoadGame);

        worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", false, true);
        continueButton = root.Q<Button>("Continue-Button");
        if(continueButton != null)
        {
            if (worlds != null && worlds.Length > 0)
            {
                continueButton.RegisterCallback<ClickEvent>(Continue);
                ToggleStyleButton(continueButton, true);
            }
        }
        else
        {
            root.Q<Label>("Load-Header").text += " - " + MyGrid.worldName;
        }
        loadMenuButton = root.Q<Button>("Load-Game-Button");
        loadMenuButton.RegisterCallback<ClickEvent>(OpenMenu);

        menu.Q<Button>("Save-Close-Button").RegisterCallback<ClickEvent>(ResetWindow);
        ResetWindow();
    }


    void OpenMenu(ClickEvent _)
    {
        worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", true, true);
        if (worlds != null && worlds.Length > 0)
        {
            menu.style.display = DisplayStyle.Flex;
        }
        else
        {
            ToggleStyleButton(loadMenuButton, false);
        }
    } 

    void ToggleStyleButton(Button button, bool activate)
    {
        if (activate)
        {
            button.RemoveFromClassList("disabled-button");
            button.AddToClassList("enabled-button");
        }
        else
        {
            button.AddToClassList("disabled-button");
            button.RemoveFromClassList("enabled-button");
        }
    }

    public void ResetWindow(ClickEvent _ = null)
    {
        selectedWorld = -1;
        selectedSave = -1;
        UpdateGrids();
        worldGroup.Init(
            (i) =>
            {
                selectedWorld = i;
                UpdateGrids();
            });

        saveGroup.Init(
            (i) =>
            {
                selectedSave = i;
                ToggleStyleButton(loadButton, selectedWorld > -1 && selectedSave > -1);
            });

        ToggleStyleButton(loadMenuButton, worlds != null && worlds.Length > 0);
        menu.style.display = DisplayStyle.None;
    }

    public void UpdateGrids()
    {
        if (selectedWorld != -1)
        {
            saveElement.AddToClassList("save-list-selected");
            worldGroup.AddToClassList("world-list-selected");
            saves = saveGroup.FillItemSource(worlds[selectedWorld].path, true, false);
            if (saves == null)
            {
                ResetWindow();
                ToggleStyleButton(loadMenuButton, false);
                return;
            }
            selectedSave = -1;
            ToggleStyleButton(loadButton, selectedWorld > -1 && selectedSave > -1);
        }
        else
        {
            saveElement.RemoveFromClassList("save-list-selected");
            worldGroup.RemoveFromClassList("world-list-selected");
        }
    }

    public void UpdateButtonState()
    {
        bool t = true;
        if(worlds.Length == 0)
        {
            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", false, true);
            if (worlds.Length == 0)
                t = false;
        }
        ToggleStyleButton(loadMenuButton, t);
    }

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
        Save save = LoadSavedData();
        if (GameObject.Find("Main Menu") == null)
        {
            await SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            Scene scene = SceneManager.GetActiveScene();
            await SceneManager.UnloadSceneAsync(scene);
        }
        else
        {
        }
        GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().
            StartLoading(save);
    }
    Save LoadSavedData()
    {
        string saveFolder = saves[selectedSave].path;
        Save save = new();
        save.worldName = SaveController.GetSaveName(worlds[selectedWorld].path);

        JsonSerializer jsonSerializer = SaveController.PrepSerializer();
        // for gridSave
        save.world = new();
        JsonTextReader jsonReader = new(new StreamReader($"{saveFolder}/Grid.json"));
        save.world.objectsSave = jsonSerializer.Deserialize<BuildsAndChunksSave>(jsonReader);
        jsonReader.Close();

        save.world.gridSave = new GridSave[MyGrid.NUMBER_OF_LEVELS];
        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            jsonReader = new(new StreamReader($"{saveFolder}/Level{i}.json"));
            save.world.gridSave[i] = jsonSerializer.Deserialize<GridSave>(jsonReader);
            jsonReader.Close();
        }

        // for playerSettings
        jsonReader = new(new StreamReader($"{saveFolder}/PlayerSettings.json"));
        save.gameState = jsonSerializer.Deserialize<GameStateSave>(jsonReader);
        jsonReader.Close();
        // for humanSaves
        jsonReader = new(new StreamReader($"{saveFolder}/Humans.json"));
        save.humans = jsonSerializer.Deserialize<HumanSave[]>(jsonReader);
        jsonReader.Close();
        // for researchCategories
        jsonReader = new(new StreamReader($"{saveFolder}/Research.json"));
        save.research = jsonSerializer.Deserialize<ResearchSave>(jsonReader);
        jsonReader.Close();
        jsonReader = new(new StreamReader($"{saveFolder}/Trade.json"));
        save.trade = jsonSerializer.Deserialize<TradeSave>(jsonReader);
        jsonReader.Close();
        return save;
    }


    private void DeleteWorld(int worldIndex)
    {
        try
        {
            Directory.Delete(worlds[worldIndex].path, true);
            worlds = worlds.Where(q => q.path != worlds[worldIndex].path).ToArray();
            worldGroup.RemoveItem(worldIndex);
            selectedWorld = -1;
            selectedSave = -1;
            if (saves.Length > 1)
                UpdateGrids();
            else
            {
                if(continueButton != null)
                    ToggleStyleButton(continueButton, false);
                ResetWindow();
            }
        }
        catch
        {
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
        catch
        {
        }
    }

    public bool IsOpen()
    {
        return menu.style.display == DisplayStyle.Flex;
    }
}
