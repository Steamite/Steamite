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
    //[SerializeField] List<Color> hatMaterial = new List<Color> { Color.azure, Color.forestGreen, Color.crimson };

    #region Grid
    public void CreateGrid(GroundLevel level, GroundLevel mainLevel, out WorldSave save)
    {

        save = new();
        save.gridSave = new GridSave[5];
        save.objectsSave = new(new BSave[] { }, new ChunkSave[] { });
        for (int i = 0; i < 5; i++)
        {
            if (i == 0)
                mainLevel.CreateGrid(save, i);
            else
                level.CreateGrid(save, i);
        }
    }


    #endregion
    #region Gane State
    public TradeSave CreateTrade(int selectedColony)
    {
        TradeHolder tradeHolder = Resources.Load<TradeHolder>($"Holders/Data/Colony Locations/{baseLocation}");
        TradeSave save = new TradeSave()
        {
            colonyLocation = tradeHolder.startingLocation.name,
            convoys = new(),
            tradeLocations = tradeHolder.tradeLocations,
            money = 2000,
            prodLevels = tradeHolder.startingLocation.config.production.Select(q => q.min).ToList(),
            statLevels = tradeHolder.startingLocation.config.stats.Select(q => q.min).ToList(),
        };
        return save;
    }

    public async Task<ResearchSave> InitResearch()
    {
        ResearchData researchData = Instantiate<ResearchData>(await Addressables.LoadAssetAsync<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset").Task);
        return new ResearchSave(researchData);
    }

    /// <summary>
    /// Called when creating a new game, creates three new Humans.
    /// </summary>
    /// <param name="humanActivation">Event that links new humans to activation.</param>
    public HumanSave[] InitHumans(int gridSize)
    {
        HumanSave[] saves = new HumanSave[3];
        GridPos pos = new(gridSize / 2, 0, gridSize / 2);
        for (int i = 0; i < 3; i++)
        {
            saves[i] = new()
            {
                color = new(hatMaterial[i]),
                gridPos = pos,
                houseID = -1,
                id = i,
                inventory = new(),
                jobSave = new() { interestID = -1, destinationID = -1, path = new(), interestType = JobSave.InterestType.Nothing, job = JobState.Free },
                objectName = $"Human {i}",
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
    #endregion
}