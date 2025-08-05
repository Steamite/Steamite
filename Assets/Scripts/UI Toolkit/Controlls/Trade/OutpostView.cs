using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class OutpostView : VisualElement
{
    public OutpostView()
    {
        style.display = DisplayStyle.Flex;
    }


    public string Open(int index)
    {
        Outpost outpost = UIRefs.TradingWindow.outposts[index];
        
        return outpost.name;
    }

    public void Hide()
    {
        style.display = DisplayStyle.None;
    }
}
