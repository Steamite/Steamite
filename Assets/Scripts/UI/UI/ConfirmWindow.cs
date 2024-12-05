using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfirmWindow : MonoBehaviour, IToolkitController
{
    public static ConfirmWindow window;
    public bool opened = false;

    [SerializeField] VisualTreeAsset dialog;
    VisualElement visualElement;
    Label head, body;
    Button confirm, cancel;
    Action resultAction;

    public void Init(VisualElement root)
    {
        window = this;
        dialog.CloneTree(root);
        visualElement = root.Q<VisualElement>("Confirm-Dialog");
        visualElement.style.display = DisplayStyle.None;
        head = visualElement.Q<Label>("Header");
        body = visualElement.Q<Label>("Body");

        confirm = visualElement.Q<Button>("Confirm");
        confirm.RegisterCallback<ClickEvent>((_) => Close(true));

        cancel = visualElement.Q<Button>("Cancel");
        cancel.RegisterCallback<ClickEvent>((_) => Close(false));
    }


    public void Open(Action _resultAction, string _head, string _body, string _confirm = "confirm", string _cancel = "cancel")
    {
        resultAction = _resultAction;
        head.text = _head;
        body.text = _body;
        confirm.text = _confirm;
        cancel.text = _cancel;
        visualElement.style.display = DisplayStyle.Flex;
        opened = true;
    }

    public void Close(bool result)
    {
        opened = false;
        if(result)
            resultAction();
        visualElement.style.display = DisplayStyle.None;
    }
}
