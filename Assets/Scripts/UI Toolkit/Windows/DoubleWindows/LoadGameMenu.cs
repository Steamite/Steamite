using RadioGroups;
using StartMenu;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadGameMenu : DoubleWindow
{
    SaveRadioList worldGroup;

    VisualElement saveElement;
    SaveRadioList saveGroup;
    Button loadButton;

    Button continueButton;
    Button loadMenuButton;

    int selectedWorld;
    int selectedSave;

    Folder[] worlds;
    Folder[] saves;

    public override void Init(VisualElement root)
    {
        base.Init(root);
        menu = root.Q<VisualElement>("Load-Menu");
        menu.Q<VisualElement>("Background").ClearClassList();
        if (isMainMenu)
        {
            worldGroup = menu.Q<SaveRadioList>("World-List");

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


            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", true);
            continueButton = root.Q<Button>("Continue-Button");
            continueButton.enabledSelf = false;
            // TODO UNCOMMENT
            /*
            if (worlds != null && worlds.Length > 0)
            {
                continueButton.RegisterCallback<ClickEvent>(Continue);
                continueButton.ToggleStyleButton(true);
            }
            else
                continueButton.ToggleStyleButton(false);*/
        }
        else
        {
            worlds = new Folder[] { new() };
            worlds[0].path = $"{Application.persistentDataPath}/saves/{MyGrid.worldName}";
            root.Q<Label>("Load-Header").text += " - " + MyGrid.worldName;
        }

        #region Save List
        saveElement = menu.Q<VisualElement>("Saves");
        saveGroup = menu.Q<SaveRadioList>("Save-List");
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
                loadButton.ToggleStyleButton(selectedWorld > -1 && selectedSave > -1);
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
        loadMenuButton.ToggleStyleButton(worlds != null && worlds.Length > 0);
        if (!isMainMenu)
            CloseWindow();
    }
    #region Window Logic
    public override void OpenWindow(ClickEvent _ = null)
    {
        if (isMainMenu)
            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", true);
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
            loadMenuButton.ToggleStyleButton(false);
        }
    }
    public override void UpdateButtonState()
    {
        bool t = true;
        if (worlds == null || worlds.Length == 0)
        {
            worlds = worldGroup.FillItemSource($"{Application.persistentDataPath}/saves", true);
            if (worlds.Length == 0)
                t = false;
        }
        loadMenuButton.ToggleStyleButton(t);
    }

    public void UpdateGrids()
    {
        if (selectedWorld != -1)
        {
            menu.Q<Label>("Hint").style.display = DisplayStyle.None;
            menu.Q<VisualElement>("List").style.display = DisplayStyle.Flex;
            saves = saveGroup.FillItemSource(worlds[selectedWorld].path, false);
            if (saves == null)
            {
                CloseWindow();
                loadMenuButton.ToggleStyleButton(false);
                return;
            }
            selectedSave = -1;
            loadButton.ToggleStyleButton(selectedWorld > -1 && selectedSave > -1);
        }
        else
        {
            menu.Q<Label>("Hint").style.display = DisplayStyle.Flex;
            menu.Q<VisualElement>("List").style.display = DisplayStyle.None;
        }
    }
    #endregion


    #region Loading Logic
    public async void Continue(ClickEvent _) => await Continue();
    public async Awaitable Continue()
    {
        string path = $"{Application.persistentDataPath}/saves";

        await Awaitable.BackgroundThreadAsync();
        worlds ??= SaveRadioList.LoadSaves(path, true);
        if (worlds.Length > 0)
        {
            saves = SaveRadioList.LoadSaves(worlds[0].path, false);
            if (saves == null)
            {
                continueButton.ToggleStyleButton(false);
                return;
            }
            await Load(saves[0].path);
            Debug.Log("Continue");
        }
    }

    async void LoadGame(ClickEvent _) => await Load();

    async Awaitable Load()
    {
        if (selectedSave == -1 || selectedSave > saves.Length)
            return;
        await Load(saves[selectedSave].path);
    }

    async Awaitable Load(string _savePath)
    {
        Binding.SetGlobalLogLevel(BindingLogLevel.All);
        await Awaitable.MainThreadAsync();
        if (GameObject.Find("Main Menu") == null)
        {
            await SceneManager.LoadSceneAsync("Loading Screen", LoadSceneMode.Additive); // scene 1 is loading screen
            await SceneManager.UnloadSceneAsync("Splash Screen");
        }
        LoadingScreen screen = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
        await screen.LoadGame(_savePath);
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
                if (isMainMenu)
                    continueButton.ToggleStyleButton(false);
                loadMenuButton.ToggleStyleButton(false);
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
                loadButton.ToggleStyleButton(selectedWorld > -1 && selectedSave > -1);
            }
            else
                DeleteWorld(selectedWorld);
        }
        catch (Exception ex)
        {
            Debug.LogError("Cannot delete save:\n" + ex);
        }
    }
    #endregion Save deletion

    #region EndMenu
    public void Test()
    {
        OpenWindow();
    }
    #endregion
}
