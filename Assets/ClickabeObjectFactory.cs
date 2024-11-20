using System;
using UnityEngine;

public class ClickabeObjectFactory : MonoBehaviour
{
    #region Y grid offset
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

    internal void CreatePipes(IProgress<int> progress, GridSave gridSave)
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
            new Vector3(gp.x, (gp.y * 2) + ROAD_OFFSET, gp.z),
            Quaternion.identity,
            MyGrid.FindLevelRoads(gp.y)).GetComponent<Road>(); // creates a road on the place of tiles

        replacement.name = replacement.name.Replace("(Clone)", "");
        if(doSet)
            MyGrid.SetGridItem(gp, replacement);
    }
}
