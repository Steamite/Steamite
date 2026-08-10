using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class PlacementChecks
{

    #region Checks
    /// <summary>
    /// Checks if a pipe can be placed on the <paramref name="pos"/> position. <br/>
    /// Must not be placed over a different pipe.
    /// </summary>
    /// <param name="pipe">Pipe to place (used here for visual effects)</param>
    /// <param name="pos">Position to check if available.</param>
    /// <returns>If it's ok to build there or not.</returns>
    public static bool CanPlacePipe(Pipe pipe, GridPos pos)
    {
        Pipe nextP = MyGrid.GetGridItem(pos, true) as Pipe;// [(int)pos.x, (int)pos.z].Pipe;

        bool canPlace =
            (nextP == null || nextP.id == -1)
                && MyGrid.GetGridItem(pos) is Road;
        //if(nextP == null || nextP.Equals(this))
        pipe.FindConnections(canPlace);/*
        else
        {
            for (int i = 0; i < 4; i++)
            {
                pipe.DisconnectPipe(i, true);
            }
        }*/
        return canPlace;
    }

    /// <summary>
    /// Checks if a <see cref="Building"/> can be placed at <paramref name="gridPos"/>. <br/>
    /// Iterates though all tiles in blueprint and marks their state.
    /// </summary>
    /// <param name="building">Building that's being placed.</param>
    /// <param name="gridPos">Anchor position</param>
    /// <returns>If it's ok to build there or not.</returns>
    public static bool CanPlaceBuilding(Building building, GridPos gridPos)
    {
        bool canBuild = true;
        RectTransform overlay = SceneRefs.Overlays
            .blueprintIndicator.MoveBlueprintOverlay(building);

        // checks all Parts of a building
        List<Road> foreignObscuredRoads = new();
        List<Image> foreignEntryOverlay = new();
        List<Image> entrances = new();
        int activeEntrances = -1;
        Vein source = null;

        Color errC, c;
        for (int i = 0; i < building.blueprint.itemList.Count; i++)
        {
            NeededGridItem item = building.blueprint.itemList[i];
            Transform tile = overlay.GetChild(i);

            GridPos itemPos = item.pos
                .Rotate(building.transform.rotation.eulerAngles.y, true);
            itemPos.x += gridPos.x;
            itemPos.z = gridPos.z - itemPos.z;

            switch (item.itemType)
            {
                case GridItemType.Road:
                    c = new(0, 1, 0, 0.25f);
                    errC = new(1, 0, 0, 0.25f);
                    CheckMassObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        foreignObscuredRoads,
                        foreignEntryOverlay);
                    break;
                case GridItemType.Anchor:
                    c = new(1, 0.843f, 0, 0.25f);
                    errC = new(1, 0.643f, 0, 0.25f);
                    CheckMassObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        foreignObscuredRoads,
                        foreignEntryOverlay);
                    break;
                case GridItemType.Entrance:
                    entrances.Add(tile.GetComponent<Image>());
                    if (MyGrid.GetGridItem(itemPos) is Road)
                        activeEntrances++;
                    break;
                case GridItemType.WaterSource:
                    c = new(0.211765f, 0.1686275f, 1, 0.25f);
                    errC = new(0.8f, 0.2196079f, 1, 0.25f);
                    CheckWaterPresence(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild);
                    break;
                case GridItemType.ResourceSource:
                    c = new(0.211765f, 0.1686275f, 1, 0.25f);
                    errC = new(0.8f, 0.2196079f, 1, 0.25f);
                    CheckVeinPresence(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        ref source);
                    break;
                case GridItemType.Pipe:
                    c = new(1f, 0.5490196f, 0f, 0.25f);
                    errC = new(1, 0, 0, 0.25f);
                    CheckMassObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        foreignObscuredRoads,
                        foreignEntryOverlay);
                    break;
                default:
                    continue;
            }
            // Move the tile up or down
            ClickableObject clickableObject = MyGrid.GetGridItem(itemPos);
            /*if (clickableObject is Rock)
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 0);
            else
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 0);*/

        }

        if (!CheckEntranceObscursion(foreignObscuredRoads, foreignEntryOverlay))
            canBuild = false;
        foreach (Image entrance in entrances)
        {
            if (activeEntrances == -1)
            {
                entrance.color = new(1f, 0.3f, 0.3f, 0.25f);
                canBuild = false;
            }
            else
                entrance.color = new(0.5f, 0.5f, 0.5f, 0.25f);
        }
        return canBuild;
    }

    static void CheckMassObscursion(GridPos pos, Image image, Color baseColor, Color errColor, ref bool canBuild, List<Road> _roads, List<Image> images)
    {
        ClickableObject clickable = MyGrid.GetGridItem(pos);
        if (clickable != null && clickable is Road)
        {
            if (MyGrid.GetGridItem(pos, true) == null)
            {
                Road road = clickable as Road;
                if (road.entryPoints.Count > 0)
                {
                    _roads.Add(road);
                    images.Add(image);
                }
                image.color = baseColor;
                return;
            }
        }
        image.color = errColor;
        canBuild = false;
    }


    static void CheckWaterPresence(
        GridPos pos, Image image,
        Color baseColor, Color errColor, ref bool canBuild)
    {
        ClickableObject clickable = MyGrid.GetGridItem(pos);
        if (clickable != null && clickable is Water water)
        {
            image.color = baseColor;
            return;
        }
        image.color = errColor;
        canBuild = false;
    }

    static void CheckVeinPresence(
        GridPos pos, Image image,
        Color baseColor, Color errColor,
        ref bool canBuild, ref Vein source)
    {
        ClickableObject clickable = MyGrid.GetGridItem(pos);
        if (clickable != null && clickable is Vein _vein)
        {
            if (source == null)
                source = _vein;
            if (source == _vein)
            {
                image.color = baseColor;
                return;
            }
        }
        image.color = errColor;
        canBuild = false;
    }

    /// <summary>
    /// Checks if the current building is not obscurring the last entry point of another building.
    /// </summary>
    /// <param name="roads">Road tiles that the building is occupying.</param>
    /// <param name="tiles">All building tiles to mark the states.</param>
    /// <returns>If it's ok to build there or not.</returns>
    static bool CheckEntranceObscursion(List<Road> roads, List<Image> tiles)
    {
        Dictionary<Building, int> buildings = new();
        bool ok = true;

        foreach (Road road in roads)
        {
            foreach (Building build in road.entryPoints)
            {
                // if not pressent already add it
                buildings.TryAdd(build, build.entryPoints.Enabled);
                
                buildings[build]--;

                if (buildings[build] == 0)
                {
                    List<int> ids = new();
                    for (int j = 0; j < roads.Count; j++)
                    {
                        if (roads[j].entryPoints.Contains(build))
                        {
                            //entries[j].gameObject.SetActive(false);
                            tiles[j].color = new(1, 0, 0, 0.5f);
                            ok = false;
                        }
                    }
                }
            }
        }
        return ok;
    }
    #endregion Checks

}

