using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class NotificationController : PanelRendererRoot
{
    private static readonly WaitForSeconds waitForMessageEnd = new WaitForSeconds(2);
    static NotificationController instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void ClearStatic() => instance = null;

    ListView view;
    readonly List<string> messages = new();


    private void Awake()
    {
        instance = this;
    }

    protected override void OnUIReload()
    {
        base.OnUIReload();
        StopAllCoroutines();
        view = Root.Q<ListView>();
        view.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        view.makeItem = () => new Label();
        view.bindItem = (el, i) => (el as Label).text = messages[i];
        view.makeNoneElement = () => null;
        view.itemsSource = messages;
        view.selectionType = SelectionType.None;
    }



    /// <inheritdoc cref="ShowMsg(string)"/>
    public static void ShowMessage(string text) => instance.ShowMsg(text);

    /// <summary>
    /// Displays a new message.
    /// </summary>
    /// <param name="text">Message text.</param>
    void ShowMsg(string text)
    {
        StartCoroutine(instance.MessageToggle(text));
    }
    /// <summary>
    /// Shows message for 2 seconds.
    /// </summary>
    /// <param name="text">Message text.</param>
    IEnumerator MessageToggle(string text)
    {
        messages.Add(text);
        view.RefreshItems();
        yield return waitForMessageEnd;
        messages.Remove(text);
        view.RefreshItems();
    }
}
