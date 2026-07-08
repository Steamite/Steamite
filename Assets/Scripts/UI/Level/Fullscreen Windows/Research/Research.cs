using BuildingStats;
using ResearchUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Overlays;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class Research : IGameDataController<ResearchSave>
{
    public ResearchNode CurrentResearch { get; private set; }

    ResearchData researchData;
    StatData statData;

    public event Action<ResearchNode> researchCompletion;
    public ResearchData GetResearchData() => researchData;

    public override async Task LoadState(ResearchSave researchSave)
    {
        researchData = Instantiate(await Addressables.LoadAssetAsync<ResearchData>(ResearchData.PATH).Task);
        List<ResearchNode> queue = new();
        for (int i = 0; i < researchSave.saveData.Count; i++)
        {
            for (int j = 0; j < researchSave.saveData[i].Count; j++)
            {
                ResearchNode node = researchData.Categories[i].Objects[j];
                node.CurrentTime = researchSave.saveData[i][j];
                node.reseachCost.Init();
            }
        }
        foreach ((int cat, int id) queueItem in researchSave.queue)
        {
            queue.Add(researchData.Categories[queueItem.cat].Objects.Find(q => q.id == queueItem.id));
        }
        if (queue.Count > 0)
            CurrentResearch = queue[0];
        statData = Instantiate(await Addressables.LoadAssetAsync<StatData>(StatData.PATH).Task);
        SceneRefs.ResearchAdapter.Init(DoResearch);
        InitResearchStats();

        AfterLoad();
    }

    /// <summary>
    /// Called by every worker in a research building
    /// </summary>
    /// <param name="efficiecy">Ammount to add.</param>
    public void DoResearch(float efficiecy)
    {
        if (CurrentResearch != null)
        {
            CurrentResearch.CurrentTime += efficiecy * 1;
        }
    }

    public void SetActive(ResearchNode newResearch)
    {
        CurrentResearch = newResearch;
    }

    void InitResearchStats()
    {
        foreach (var categ in researchData.Categories)
        {
            foreach (var node in categ.Objects)
            {
                if (node.nodeType == NodeType.Stat)
                {
                    Stat stat = statData.GetObjectBySaveIndex(node.objectConnection); //q => q.id == node.objectConnection.objectId);
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

    public void FinishResearch()
    {
        NotificationController.ShowMessage($"Research Finished {CurrentResearch.Name}");
        CurrentResearch = null;
        // TODO: Assign new one
        researchCompletion?.Invoke(CurrentResearch);
    }

    public override ResearchSave SaveState()
    {
        ResearchSave save = new()
        {
            saveData = new(),
            queue = new(),

            count = 0
        };

        for (int i = 0; i < researchData.Categories.Count; i++)
        {
            List<float> saves = new();
            for (int j = 0; j < researchData.Categories[i].Objects.Count; j++)
            {
                saves.Add(researchData.Categories[i].Objects[j].CurrentTime);
                if (researchData.Categories[i].Objects[j].Equals(CurrentResearch))
                    save.queue.Add(new(i, researchData.Categories[i].Objects[j].id));
            }
            save.saveData.Add(saves);
        }
        return save;
    }
}