using TradeData.Locations;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TextFieldLabel : TextField, IUIElement
{
    INameChangable inspectedObject;
    bool canChange;
    public TextFieldLabel()
    {
        value = "PlaceHolder";
        pickingMode = PickingMode.Ignore;
        selectAllOnMouseUp = false;
        selectAllOnFocus = true;
        doubleClickSelectsWord = false;
        tripleClickSelectsLine = false;
        isReadOnly = true;


        VisualElement element = hierarchy[0];
        element.pickingMode = PickingMode.Ignore;
        element.style.borderBottomWidth = 0;
        element.style.borderTopWidth = 0;
        element.style.borderLeftWidth = 0;
        element.style.borderRightWidth = 0;
        element.style.backgroundColor = new Color(0, 0, 0, 0);
        element.style.paddingBottom = 0;
        element.style.paddingTop = 0;
        element.style.paddingLeft = 0;
        element.style.paddingRight = 0;

        TextElement el = hierarchy[0][0] as TextElement;
        el.style.unityTextAlign = TextAnchor.MiddleCenter;
        el.style.fontSize = 40;
        el.style.flexGrow = 1;
        el.RegisterCallback<MouseDownEvent>(Click);
    }

    public void Open(object _inspectedObject)
    {
        inspectedObject = _inspectedObject as INameChangable;
        value = inspectedObject.Name;

        canChange = _inspectedObject is not ColonyLocation;
        if (canChange)
        {
        }
    }

    void Click(MouseDownEvent ev)
    {
        if (ev.clickCount == 2 && canChange)
        {
            RegisterCallback<FocusOutEvent>(OnChange);
            isReadOnly = false;
            SelectAll();
            MarkDirtyRepaint();
            MainShortcuts.DisableAll();
        }
    }

    void OnChange(FocusOutEvent focusOutEvent)
    {
        Debug.Log("change" + inspectedObject);
        if (inspectedObject != null)
        {
            value = value.Trim(' ');
            if (value != "")
                inspectedObject.Name = value;
            else
                value = inspectedObject.Name;
            isReadOnly = true;
        }
        UnregisterCallback<FocusOutEvent>(OnChange);
        MainShortcuts.EnableAll();
    }
}
