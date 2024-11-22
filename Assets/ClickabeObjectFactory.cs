using System;
using System.Linq;
using UnityEngine;

public class ClickabeObjectFactory : MonoBehaviour
{
    #region Y grid offset
    public const int LEVEL_HEIGHT = 2;

    public const int BUILD_OFFSET = 1;
    public const int CHUNK_OFFSET = 1;
    public const int HUMAN_OFFSET = 1;

    public const float ROCK_OFFSET = 1.5f;
    public const float ROAD_OFFSET = 0.45f;
    #endregion
    public ResourceHolder buildPrefabs;
    public ResourceHolder tilePrefabs;
    public ResourceHolder specialPrefabs;

    public Chunk CreateAChunk(GridPos gridPos, Resource startingResource)
    {
        Chunk chunk = Instantiate(specialPrefabs.GetPrefab("Chunk"), gridPos.ToVec(CHUNK_OFFSET), Quaternion.identity, MyGrid.FindLevelChunks(gridPos.y)).GetComponent<Chunk>();
        chunk.Init(startingResource);

        return chunk;
    }

    public void CreatePipes(IProgress<int> progress, WorldSave gridSave)
    {
        /*GameObject pipePref = buildPrefabs.GetPrefab("Fluid Pipe base").gameObject;
        for (int x = 0; x < gridSave.height; x++)
        {
            for (int z = 0; z < gridSave.width; z++)
            {
                if (gridSave.pipes[x, z] != null)
                {
                    Instantiate(pipePref, new Vector3(x, , z), Quaternion.identity, SceneRefs.levels[0].pipes)
                        .GetComponent<Pipe>().Load(gridSave.pipes[x, z]);
                }
                progress.Report(progressGlobal += 2);
            }
        }*/
    }

    public void CreateRoad(GridPos gp, bool doSet)
    {
        Road replacement = Instantiate(
            tilePrefabs.GetPrefab("Road").gameObject,
            new Vector3(gp.x, (gp.y * LEVEL_HEIGHT) + ROAD_OFFSET, gp.z),
            Quaternion.identity,
            MyGrid.FindLevelRoads(gp.y)).GetComponent<Road>(); // creates a road on the place of tiles

        replacement.name = replacement.name.Replace("(Clone)", "");
        if(doSet)
            MyGrid.SetGridItem(gp, replacement);
    }

    #region Loading Game
    public Building CreateSavedBuilding(BSave save)
    {
        Building _pref = buildPrefabs.GetPrefab(save.prefabName) as Building; // find the prefab

        GridPos rotate = MyGrid.Rotate(save.build.blueprint.moveBy, transform.rotation.eulerAngles.y);
        Building b = Instantiate(_pref,
            new Vector3(save.gridPos.x + rotate.x, (save.gridPos.y * LEVEL_HEIGHT) + BUILD_OFFSET, save.gridPos.z + rotate.z),
            Quaternion.Euler(0, save.rotationY, 0),
            MyGrid.FindLevelBuildings(save.gridPos.y));
        b.Load(save);
        MyGrid.PlaceBuild(b, true);
        return b;
    }

    public Rock CreateSavedRock(RockSave save, GridPos gp)
    {
        Rock rock = Instantiate(
            tilePrefabs.GetPrefab(save.oreName), 
            new Vector3(gp.x, (gp.y * LEVEL_HEIGHT) + ROCK_OFFSET, gp.z), Quaternion.identity, 
            MyGrid.FindLevelRocks(gp.y)).GetComponent<Rock>();
        rock.Load(save);
        MyGrid.SetGridItem(gp, rock);
        if (rock.toBeDug)
        {
            SceneRefs.gridTiles.toBeDigged.Add(rock);
            SceneRefs.gridTiles.HighLight(SceneRefs.gridTiles.toBeDugColor, rock.gameObject);
        }
        return rock;
    }

    public Water CreateSavedWater(WaterSave save, GridPos gp)
    {
       Water water = Instantiate(
            tilePrefabs.GetPrefab("Water").gameObject,
            new Vector3(gp.x, (gp.y * LEVEL_HEIGHT), gp.z),
            Quaternion.identity,
            MyGrid.FindLevelWater(gp.y)).GetComponent<Water>();
        water.Load(save);
        MyGrid.SetGridItem(gp, water);
        return water;
    }

    public Human CreateSavedHuman(HumanSave save)
    {
        int parent = save.workplaceId > -1 ? 1 : 0;
        Human human = GameObject.Instantiate(specialPrefabs.GetPrefab("Human"),
            new Vector3(save.gridPos.x, (save.gridPos.y * LEVEL_HEIGHT) + HUMAN_OFFSET, save.gridPos.z),
            Quaternion.identity,
            SceneRefs.humans.transform.GetChild(parent)).GetComponent<Human>();
        human.transform.GetChild(1).GetComponent<MeshRenderer>().material.color = save.color.ConvertColor();
        human.id = save.id;
        human.jData = new(save.jobSave, human);
        human.name = save.name;
        human.inventory = save.inventory;
        human.specialization = save.specs;
        if (human.jData.path.Count > 0)
            human.ChangeAction(HumanActions.Move);
        else
            human.Decide();
        // house assigment
        if (save.houseID != -1)
        {
            human.home = MyGrid.buildings.Where(q => q.id == save.houseID).
                SingleOrDefault().GetComponent<House>();
            human.home.assigned.Add(human);
        }
        // workplace assigment
        if (save.workplaceId != -1)
        {
            human.workplace = MyGrid.buildings.Where(q => q.id == save.workplaceId).
                SingleOrDefault().GetComponent<ProductionBuilding>();
            human.workplace.assigned.Add(human);
        }
        return human;
    }
    #endregion Loading Game
}
