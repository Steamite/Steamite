using BuildingStats;
using ResearchUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.PanelRenderer;

[RequireComponent(typeof(Research))]
public class ResearchWindow : FullscreenWindow
{
    IUIElement UI;
    Research research;

    private void Awake()
    {
        research = GetComponent<Research>();
        AddOnLoad(ref research.OnLoad);
    }

    protected override void OnUIReload()
    {
        UI = Root.Q<TabView>() as IUIElement;
        base.OnUIReload();
    }

    protected override void OnDataLoadLogic()
    {
        ResearchData data = research.GetResearchData();
        ((IInitiableUI<ResearchData>)UI).Init(data);
    }





    public override void OpenWindow()
    {
        base.OpenWindow();
        UI.Open(null);
    }

    public void OpenWithFocus(BuildingWrapper wrapper)
    {
        base.OpenWindow();
        int i = 0, j = 0;
        List<ResearchCategory> data = research.GetResearchData().Categories;
        foreach (var cat in data)
        {
            for (j = 0; j < cat.Objects.Count; j++)
            {
                if (cat.Objects[j].nodeType == NodeType.Building
                    && cat.Objects[j].objectConnection.objectId == wrapper.id)
                {
                    int x = cat.Objects.FindIndex(q => q.level == cat.Objects[j].level);
                    UI.Open((i, cat.Objects[j].level + 1, j - x));
                    return;
                }
            }
            i++;
        }
    }
}
