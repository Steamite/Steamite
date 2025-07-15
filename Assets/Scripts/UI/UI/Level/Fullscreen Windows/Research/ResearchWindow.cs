using BuildingStats;
using ResearchUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class ResearchWindow : FullscreenWindow
{
    IUIElement UI;

    public ResearchNode currentResearch { get; private set; }
    [HideInInspector] public ResearchData researchData;
    [HideInInspector] public BuildingStats.StatData statData;
    public event Action<ResearchNode> researchCompletion;

    public override void GetWindow()
    {
        base.GetWindow();
        UI = window.Q<TabView>() as IUIElement;
        ((IInitiableUI)UI).Init();
        window.style.display = DisplayStyle.Flex;
        window.schedule.Execute(() => window.style.display = DisplayStyle.None).ExecuteLater(15);
    }

    public async void LoadGame(ResearchSave researchSave)
    {
        researchData = Instantiate<ResearchData>(await Addressables.LoadAssetAsync<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset").Task);
        List<ResearchNode> queue = new();
        for (int i = 0; i < researchSave.saveData.Count; i++)
        {
            for (int j = 0; j < researchSave.saveData[i].Count; j++)
            {
                ResearchNode node = researchData.Categories[i].Objects[j];
                node.CurrentTime = researchSave.saveData[i][j];
            }
        }
        foreach ((int cat, int id) queueItem in researchSave.queue)
        {
            queue.Add(researchData.Categories[queueItem.cat].Objects.Find(q => q.id == queueItem.id));
        }
        if (queue.Count > 0)
            currentResearch = queue[0];
        Init();
    }

    public async void Init()
    {
        statData = Instantiate(await Addressables.LoadAssetAsync<BuildingStats.StatData>("Assets/Game Data/Research && Building/Stats.asset").Task);
        GetWindow();
        SceneRefs.ResearchAdapter.Init(DoResearch);
        ((IInitiableUI)UIRefs.bottomBar.Q<VisualElement>(className: "build-menu")).Init();
        InitResearchStats();
    }

    void InitResearchStats()
    {
        foreach (var categ in researchData.Categories)
        {
            foreach (var node in categ.Objects)
            {
                if (node.nodeType == NodeType.Stat)
                {
                    int i = statData.Categories[node.nodeCategory].Objects.FindIndex(q => q.id == node.nodeAssignee);
                    Stat stat = statData.Categories[node.nodeCategory].Objects[i];
                    if (node.researched)
                    {
                        stat.AddEffect();
                    }
                    else
                    {
                        node.RegisterFinishCallback(stat.AddEffect);
                    }
                }
            }
        }
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        UI.Open(researchData);
    }

    public void OpenWindow(BuildingWrapper wrapper)
    {
        base.OpenWindow();
        int i = 0, j = 0;
        foreach (var cat in researchData.Categories)
        {
            for (j = 0; j < cat.Objects.Count; j++)
            {
                if (cat.Objects[j].nodeType == NodeType.Building
                    && cat.Objects[j].nodeAssignee == wrapper.id)
                {
                    int x = cat.Objects.FindIndex(q => q.level == cat.Objects[j].level);
                    UI.Open((i, cat.Objects[j].level + 1, j - x));
                    return;
                }
            }
            i++;
        }
    }

    public void FinishResearch()
    {
        SceneRefs.ShowMessage($"Research Finished {currentResearch.Name}");
        currentResearch = null;
        // TODO: Assign new one
        researchCompletion?.Invoke(currentResearch);
    }

    /// <summary>
    /// Called by every worker in a research building
    /// </summary>
    /// <param name="efficiecy">Ammount to add.</param>
    public void DoResearch(float efficiecy)
    {
        if (currentResearch != null)
        {
            currentResearch.CurrentTime += efficiecy * 1;
        }
    }

    public void SetActive(ResearchNode newResearch)
    {
        currentResearch = newResearch;
    }
}
