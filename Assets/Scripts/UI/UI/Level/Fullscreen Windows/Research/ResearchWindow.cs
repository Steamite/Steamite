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
    public event Action<ResearchNode> researchCompletion;

    public override void GetWindow()
    {
        base.GetWindow();
        UI = window.Q<TabView>() as IUIElement;
        ((IInitiableUI)UI).Init();
    }
    public async void NewGame()
    {
        researchData = Instantiate<ResearchData>(await Addressables.LoadAssetAsync<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset").Task);
        Init();
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
        foreach ((int cat, int index) queueItem in researchSave.queue)
        {
            queue.Add(researchData.Categories[queueItem.cat].Objects[queueItem.index]);
        }
        if (queue.Count > 0)
            currentResearch = queue[0];
        Init();

    }


    public void Init()
    {
        GetWindow();
        SceneRefs.researchAdapter.Init(DoResearch);
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        UI.Open(researchData);
    }

    public void FinishResearch()
    {
        SceneRefs.ShowMessage($"Research Finished {currentResearch.nodeName}");
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
