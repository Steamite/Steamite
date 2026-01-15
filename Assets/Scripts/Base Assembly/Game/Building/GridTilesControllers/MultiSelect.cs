using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MultiSelect : MonoBehaviour
{
    GridPos startPos;
    private bool deselect;

    /// <summary>Tiles marked while dragging.</summary>
    List<ClickableObject> tempMarkedTiles = new();
    List<GridPos> tempMarkedTilePos = new();
    List<int> checkpoints = new();
    List<List<ClickableObject>> markedTiles = new();


    /// <summary>Color for selecting what do dig.</summary>
    public Color toBeDugColor = (Color.yellow + Color.red) / 2;

    /// <summary>
    /// Called when canceling drag, changes highlight of all rocks in markedTiles.
    /// </summary>
    void ClearMarks()
    {
        if (tempMarkedTiles != null)
        {
            foreach (Rock r in tempMarkedTiles)
            {
                Color c = new();
                if (r.toBeDug)
                    c = (Color.yellow + Color.red) / 2;
                r.Highlight(c);
            }
        }
    }
    /// <summary>
    /// ReMarks all rock in range from start to activePos. 
    /// </summary>
    public void CalcTiles(GridPos activePos)
    {
        ClearMarks();
        List<ClickableObject> rocks = new();
        float x = (Mathf.FloorToInt(startPos.x) - activePos.x) / 2f;
        float z = (Mathf.FloorToInt(startPos.z) - activePos.z) / 2f;
        rocks.AddRange(Physics.OverlapBox(new Vector3(startPos.x - x, (startPos.y * 2) + ClickableObjectFactory.ROCK_OFFSET, startPos.z - z), new(Mathf.Abs(x), 0.5f, Mathf.Abs(z))).Where(q => q.GetComponent<Rock>() != null).Select(q => q.GetComponent<Rock>()).ToList());
        List<ClickableObject> filtered = rocks.ToList();
        List<Rock> toBeDug = SceneRefs.JobQueue.toBeDug;
        foreach (Rock g in rocks)
        {
            if (!deselect && toBeDug.Contains(g))
            {
                filtered.Remove(g);
            }
            g.Highlight(deselect ? (Color.red / 2) : toBeDugColor);
        }
        tempMarkedTiles = filtered;
    }

    /// <summary>
    /// Changes the state of rocks in <see cref="tempMarkedTiles"/>, if the first one was marked, cancels them.<br/>
    /// Else marks orders their excavation.
    /// </summary>
    public void DigMark()
    {
        List<Rock> toBeDug = SceneRefs.JobQueue.toBeDug;
        HumanUtil humans = transform.parent.parent.GetChild(2).GetComponent<HumanUtil>();
        if (deselect)
        {
            foreach (Rock markTile in tempMarkedTiles.Select(q => q.GetComponent<Rock>())) // removes to be dug
            {
                toBeDug.RemoveAll(q => q == markTile);
                markTile.toBeDug = false;
                markTile.Highlight(new());
                SceneRefs.JobQueue.CancelJob(JobState.Digging, markTile);
                markTile.Assigned?.SetJob(JobState.Free);
            }
        }
        else
        {
            foreach (var dig in toBeDug) // removes to be dug
            {
                tempMarkedTiles.RemoveAll(q => q == dig);
            }
            foreach (Rock tile in tempMarkedTiles)
            {
                toBeDug.Add(tile); // add rock
                tile.toBeDug = true;
                SceneRefs.JobQueue.AddJob(JobState.Digging, tile);
            }
        }
        tempMarkedTiles.Clear();
        deselect = false;
    }

    /// <summary>
    /// Creates/Deletes pipes to copy the shortest path from startPos to activePos
    /// </summary>
    public void CalcPipes(GridPos activePos, Pipe prefab)
    {
        Transform pipes = MyGrid.FindLevelPipes(startPos.y / 2);

        int i = tempMarkedTilePos.IndexOf(activePos);
        if (i == -1)
        {
            if (MyGrid.GetGridItem(activePos) is Road)
            {
                List<GridPos> partPath = PathFinder.FindPath(startPos, activePos, typeof(Road));
                if (partPath == null)
                    return;
                if (markedTiles.Count > 0)
                    partPath.RemoveAt(0);
                for (int j = tempMarkedTilePos.Count - 1; j >= 0; j--)
                {
                    int k = partPath.IndexOf(tempMarkedTilePos[j]);
                    if (k == -1)
                    {
                        (tempMarkedTiles[j] as Building).DestoyBuilding();
                    }
                    else
                        partPath.RemoveAt(k);
                }

                for (int j = 0; j < partPath.Count; j++)
                {
                    AddPipe(partPath[j], pipes, prefab);
                }
            }
        }
        else
        {
            for (int j = tempMarkedTiles.Count - 1; j > i; j--)
            {
                (tempMarkedTiles[j] as Building).DestoyBuilding();
            }
        }
        MyGrid.GetOverlay().MovePlacePipeOverlay(activePos, false);
    }
    void AddPipe(GridPos pos, Transform pipes, Pipe prefab)
    {
        if(MyGrid.GetGridItem(pos, true) != null)
        {
            return;
        } 
        Pipe pipe = Instantiate(prefab, pos.ToVec(ClickableObjectFactory.PIPE_OFFSET), Quaternion.identity, pipes);
        pipe.gameObject.layer = 2;
        pipe.maximalProgress = pipe.CalculateMaxProgress();
        pipe.GetRenderComponents();
        pipe.ChangeRenderMode(true);
        pipe.Highlight(pipe.CanPlace(false) && MyRes.CanAfford(pipe.Cost * (tempMarkedTiles.Count + 1 + markedTiles.SelectMany(q => q).Count())) ? Color.blue : Color.red);
        MyGrid.SetGridItem(pos, pipe, true);
        tempMarkedTiles.Add(pipe);
        tempMarkedTilePos.Add(pos);
    }

    public void MarkPipeCheckpoint(GridPos pos)
    {
        markedTiles.Add(tempMarkedTiles.ToList());

        tempMarkedTiles.Clear();
        //tempMarkedTiles.Add(markedTiles.Last().Last());

        tempMarkedTilePos.Clear();
        //tempMarkedTilePos.Add(pos);
        startPos = pos;

        MyGrid.GetOverlay().AddCheckPointTile(pos);
    }

    public void InitDig(GridPos pos, Rock r)
    {
        startPos = pos;
        deselect = r.toBeDug ? true : false;
    }

    public void InitPipes(GridPos gridPos, Pipe pipe)
    {
        startPos = gridPos;
        tempMarkedTiles.Clear();
        tempMarkedTiles.Add(pipe);

        tempMarkedTilePos.Clear();
        tempMarkedTilePos.Add(startPos);
    }

    public bool ClickPipes(GridPos gridPos)
    {
        if (tempMarkedTiles.Count > 1)
        {
            MarkPipeCheckpoint(gridPos);
            return false;
        }
        else
        {
            foreach (Building b in markedTiles.SelectMany(q => q).Union(tempMarkedTiles))
            {
                if (b.CanPlace(true))
                    b.PlaceBuilding();
                else
                    b.DestoyBuilding();
            }

            markedTiles.Clear();
            tempMarkedTiles.Clear();
            return true;
        }
    }

    public bool Break()
    {
        if(markedTiles.Count == 0)
            return true;

        for (int i = tempMarkedTiles.Count-1; i > -1; i--)
        {
            (tempMarkedTiles[i] as Pipe).DestoyBuilding();
        }

        int count = markedTiles.Count;
        tempMarkedTiles = markedTiles[count - 1];
        tempMarkedTilePos = tempMarkedTiles.Select(q => q.GetPos()).ToList();

        markedTiles.RemoveAt(count - 1);
        startPos = tempMarkedTilePos[0];
        SceneRefs.CameraSceneMover.MoveToPosition(tempMarkedTilePos[^1], true);
        MyGrid.GetOverlay().RemoveCheckPointTile(count + 1);
        return false;
    }

    public void ClearDig()
    {
        foreach (Rock r in tempMarkedTiles)
        {
            r.Highlight(new());
        }
        tempMarkedTiles.Clear();
    }

    public void ClearPipes()
    {
        List<Pipe> pipes = tempMarkedTiles.Union(markedTiles.SelectMany(q => q)).Cast<Pipe>().ToList();
        for (int i = pipes.Count - 1; i > -1; i--)
        {
            pipes[i].DestoyBuilding();
        }
        MyGrid.GetOverlay().DestroyBuilingTiles();
        markedTiles.Clear();
        tempMarkedTiles.Clear();
    }

    public void RemoveFromMarked(ClickableObject cO)
    {
        int i = tempMarkedTiles.IndexOf(cO);
        if(i > -1)
        {
            tempMarkedTiles.RemoveAt(i);
            if (tempMarkedTilePos.Count > i)
                tempMarkedTilePos.RemoveAt(i);
        }
    }
}
