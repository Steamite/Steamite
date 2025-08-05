using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveDialog : MonoBehaviour, IGridMenu
{
    [SerializeField] UIDocument uiDoc;
    public bool opened;

    Button saveButton;
    Button closeButton;

    string worldName;
    string saveName;
    [CreateProperty]
    public string SaveName
    {
        get { return saveName; }
        set
        {
            saveName = value;
            UpdateButtonState();
        }
    }

    Action<string> saveAction;
    public void Init(Action<string> _saveAction)
    {
        saveAction = _saveAction;
        TextField f = uiDoc.rootVisualElement.Q<TextField>("SaveField");
        f.dataSource = this;
        saveButton = uiDoc.rootVisualElement.Q<Button>("Save-Game");
        closeButton = uiDoc.rootVisualElement.Q<Button>("Cancel-Save");

        closeButton.RegisterCallback<ClickEvent>(CloseWindow);

        Button b = uiDoc.rootVisualElement.Q<Button>("Save");
        b.RegisterCallback<ClickEvent>((e) => OpenWindow(e));
        saveName = "";
    }

    public void OpenWindow(ClickEvent _)
    {
        uiDoc.rootVisualElement.Q<VisualElement>("Save-Dialog").style.display = DisplayStyle.Flex;
        saveButton.RegisterCallback<ClickEvent>(SaveGame);
        opened = true;
    }

    public void CloseWindow(ClickEvent _ = null)
    {
        uiDoc.rootVisualElement.Q<TextField>("SaveField").value = "";
        uiDoc.rootVisualElement.Q<VisualElement>("Save-Dialog").style.display = DisplayStyle.None;
        opened = false;
        saveName = "";
        saveButton.UnregisterCallback<ClickEvent>(SaveGame);
    }

    public void UpdateButtonState()
    {
        if (saveName.Length > 0)
        {
            saveButton.RemoveFromClassList("disabled-button");
            saveButton.AddToClassList("enabled-button");
        }
        else
        {
            saveButton.AddToClassList("disabled-button");
            saveButton.RemoveFromClassList("enabled-button");
        }
    }

    public void SaveGame(ClickEvent _)
    {
        if (saveName.Length > 0)
            saveAction(saveName);
    }

    public bool IsOpen()
    {
        return opened;
    }

    public void OpenWindow()
    {
        throw new NotImplementedException();
    }
}
