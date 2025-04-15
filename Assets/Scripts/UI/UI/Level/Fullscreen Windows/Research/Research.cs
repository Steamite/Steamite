using System.Threading.Tasks;
using TMPro;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class Research : FullscreenWindow
{
	IUIElement UI;
    public ResearchNode currentResearch;
    public StorageResource researchResourceInput;
    public ResearchData researchData;
    public BuildButtonHolder buildData;

	public override void GetWindow()
	{
		base.GetWindow();
        UI = window.Q<TabView>() as IUIElement;
		((IInitiableUI)UI).Init();
	}
	public async void NewGame()
	{
		researchData = await Addressables.LoadAssetAsync<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset").Task;
        buildData = await Addressables.LoadAssetAsync<BuildButtonHolder>("Assets/Game Data/Research && Building/Build Data.asset").Task;
        Init();
	}

	public void LoadGame(ResearchSave researchSave)
	{
		//TradeHolder tradeHolder = Resources.Load<TradeHolder>($"Holders/Data/Colony Locations/{tradeSave.colonyLocation}");
        
		Init();
	}


	public void Init()
    {
        GetWindow();
        
        SceneRefs.researchAdapter.Init(DoResearch, DisplayInfoWindowDetails);
    }

	public override void OpenWindow()
	{
		base.OpenWindow();
        UI.Open(researchData);
	}
	ResearchDispayData DisplayInfoWindowDetails()
    {
        ResearchDispayData data = new();
        /*if (currentResearch)
        {
            data.name = currentResearch.name;
            data.progress = $"{(currentResearch.node.currentTime/(float)currentResearch.node.researchTime):0%}  %";
        }
        else
        {
            data.name = "None";
            data.progress = "0 %";
        }
        data.color = Random.ColorHSV();*/
        return data;
    }
/*
    //Start researching a research
    public void StartResearch(ResearchUIButton button)
    {
        if (button == currentResearch)
            return;
        if (currentResearch == null)
        {
            currentResearch = button;
            currentResearch.StartAnim();
        }
        else if (button != currentResearch)
        {
            currentResearch.EndAnim(true);
            currentResearch = button;
            currentResearch.StartAnim();
            // confirmation popup
        }
        UI.StartCoroutine(UI.UpdateButtonFill());
        UI.openResearchAnimator.SetFloat("Speed", 0.5f);
        UI.openResearchAnimator.SetTrigger("selected");
    }*/

    public void FinishResearch()
    {
        /*currentResearch.Complete();
        currentResearch = null;*/
    }
    
    /// <summary>
    /// Called by every worker in a research building
    /// </summary>
    /// <param name="efficiecy">Ammount to add.</param>
    public void DoResearch(float efficiecy)
    {
        /*if (currentResearch)
        {
            currentResearch.node.currentTime += efficiecy * 1;
        }*/
    }
}
