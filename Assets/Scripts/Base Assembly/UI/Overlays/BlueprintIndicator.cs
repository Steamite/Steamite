using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintIndicator : MonoBehaviour
{
    /// <summary>Transform for build placing overlay.</summary>
    RectTransform blueprintEntryPoints = null;

    #region Buildings
    /// <summary>
    /// Moves placing grid and if needed fill it.
    /// </summary>
    /// <param name="building">Buidling to move with.</param>
    public RectTransform MoveBlueprintOverlay(Building building)
    {
        if (blueprintEntryPoints == null)
        {
            GridPos pos = building.GetPos();
            blueprintEntryPoints = OverlayUtils.CreateGroup(
                transform as RectTransform,
                pos,
                "BlueprintGroup");
            blueprintEntryPoints.anchoredPosition3D = new(
                blueprintEntryPoints.anchoredPosition.x,
                blueprintEntryPoints.anchoredPosition.y,
                -(pos.y*2) - 0.501f);
            blueprintEntryPoints.gameObject.layer = LayerMask.NameToLayer("Entry Points");

            
            foreach (NeededGridItem item in building.blueprint.itemList)
            {
                GridPos itemPos = item.pos;
                itemPos.z = -itemPos.z;
                Image tile = OverlayUtils.CreateTile(
                    itemPos, 
                    blueprintEntryPoints, 
                    item.itemType.ToString());

                Color color = new();
                switch (item.itemType)
                {
                    case GridItemType.WaterSource:
                        color = Color.blue;
                        break;
                    case GridItemType.ResourceSource:
                        color = Color.black;
                        break;
                    case GridItemType.Entrance:
                        color = Color.grey;
                        break;
                    case GridItemType.Anchor:
                        color = Color.yellow;
                        break;
                    case GridItemType.Pipe:
                        color = Color.darkViolet;
                        break;
                }
                tile.color = color;
            }
        }

        GridPos gp = building.blueprint.moveBy.Rotate(building.transform.rotation.eulerAngles.y);

        blueprintEntryPoints.localRotation =
            Quaternion.Euler(180, 0, building.transform.rotation.eulerAngles.y);

        blueprintEntryPoints.anchoredPosition = new(
            building.transform.position.x - gp.x,
            building.transform.position.z - gp.z);

        return blueprintEntryPoints;
    }

    /// <summary>Clears all tiles from the <see cref="overlay"/>.</summary>
    public void DestroyBuilingTiles()
    {
        if (!blueprintEntryPoints)
            return;
        Destroy(blueprintEntryPoints.gameObject);
        blueprintEntryPoints = null;
    }
    #endregion Buildings

    #region Pipes
    public void MovePlacePipeOverlay(GridPos pos, bool init)
    {
        if (init == true)
        {
            GameObject entryPoints = new GameObject("pipe-checkpoints", typeof(RectTransform));
            entryPoints.transform.parent = transform;
            (entryPoints.transform as RectTransform).anchoredPosition = new(0, 0);
            AddCheckPointTile(pos);
            AddCheckPointTile(pos);
        }
        else
        {
            blueprintEntryPoints.GetChild(1).GetComponent<RectTransform>()
                .anchoredPosition = new(pos.x, -pos.z);
        }
    }

    public void AddCheckPointTile(GridPos pos)
    {
        Image image = OverlayUtils.CreateTile(pos, blueprintEntryPoints);
        image.color = new Color(0.5803922f, 0f, 0.8274511f, 0.25f);
    }

    public void RemoveCheckPointTile(int i)
    {
        Destroy(blueprintEntryPoints.GetChild(i).gameObject);
        if (i == 0)
            Destroy(blueprintEntryPoints.gameObject);
    }

    public RectTransform GetTile(int childIndex)
    {
        return blueprintEntryPoints.GetChild(childIndex) as RectTransform;
    }
    #endregion Pipes
}
