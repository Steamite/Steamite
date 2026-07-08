using ResearchUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NewGameInit : MonoBehaviour
{
    [SerializeField] string baseLocation = "Highlands";
    [SerializeField] List<Color> hatMaterial = new List<Color> { Color.azure, Color.forestGreen, Color.crimson };

    [SerializeField] int numberOfDays = 6;
    [SerializeField] int dayTime = 6 * 60;
    [SerializeField] List<JobState> priority;

    [SerializeField] int numberOfHumans = 3;

    #region Grid
    public void CreateGrid(List<GroundLevel> levelPrefabs, out WorldSave save)
    {
        save = new();
        save.gridSave = new GridSave[MyGrid.NUMBER_OF_LEVELS];
        save.objectsSave = new(new BuildingSave[] { }, new ChunkSave[] { }, new VeinSave[] { });
        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            levelPrefabs[i].CreateGrid(save, i);
        }
    }


    #endregion
    #region Game State
    public async Task<TradeSave> InitTrade(int selectedColony)
    {
        TradeHolder tradeHolder = await Addressables.LoadAssetAsync<TradeHolder>($"Colony Locations/{baseLocation}.asset").Task;
        TradeSave save = new TradeSave()
        {
            colonyLocation = tradeHolder.startingLocation.Name,
            convoys = new(),
            tradeLocations = tradeHolder.tradeLocations.Select(q => new TradeLocationSave(q)).ToList(),
            money = 2000,
            prodLevels = tradeHolder.startingLocation.config.production.Select(q => q.min).ToList(),
            statLevels = tradeHolder.startingLocation.config.stats.Select(q => q.min).ToList(),
            outposts = new()
            {
                new("Outpost 1"),
                new("Outpost 2"),
                new("Outpost 3")
            }
        };
        return save;
    }

    public async Task<ResearchSave> InitResearch()
    {
        ResearchData researchData = Instantiate(await Addressables.LoadAssetAsync<ResearchData>(ResearchData.PATH).Task);
        return new ResearchSave(researchData);
    }

    /// <summary>
    /// Called when creating a new game, creates three new Humans.
    /// </summary>
    /// <param name="humanActivation">Event that links new humans to activation.</param>
    public HumanSave[] InitHumans(int gridSize)
    {
        HumanSave[] saves = new HumanSave[numberOfHumans];
        GridPos pos = new(gridSize / 2, 0, gridSize / 2);
        for (int i = 0; i < numberOfHumans; i++)
        {
            saves[i] = new()
            {
                color = new(hatMaterial[i % hatMaterial.Count]),
                gridPos = pos,
                houseID = -1,
                id = i,
                inventory = new(),
                jobSave = new() { interestID = -1, destinationID = -1, path = new(), interestType = JobSave.InterestType.Nothing, job = JobState.Free },
                name = $"Human {i}",
                sleep = 10,
                specs = Specializations.Worker,
                workplaceId = -1
            };
        }
        return saves;
    }

    public GameStateSave SetNewGameState()
    {
        return new()
        {
            autoSave = false,
            dayTime = dayTime,
            numberOfDays = numberOfDays,
            priorities = priority
        };
    }

    public async Task<QuestControllerSave> InitQuests(bool randomMap)
    {
        QuestHolder quest = await Addressables.LoadAssetAsync<QuestHolder>("QuestData").Task;
        QuestControllerSave questSave = new()
        {
            activeQuests = randomMap ? new() : new() { new(quest.Categories[0].Objects[0]) { state = QuestState.Active } },
            finishedQuests = new(),
            order = new(quest.Categories[2].Objects[0]) { state = QuestState.Active },
            trust = 40,
            finishedOrdersCount = 0,
            orderChoiceSaves = new()
        };
        return questSave;
    }
    #endregion
}