using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Provides visualization for placing new buildings and entrypoints.</summary>
public class UIOverlay : MonoBehaviour
{
    #region Variables
    /// <summary>Prefab for tile overlays.</summary>
    [SerializeField] Image overlayTile;
    /// <summary>Group for build placing overlay.</summary>
    [SerializeField] public RectTransform overlayParent;
    /// <summary>Group for entry point overlays.</summary>
    [SerializeField] public RectTransform entryPointParent;
    /// <summary>Prefab for tile groups.</summary>
    [SerializeField] public GameObject groupPrefab;
    /// <summary>List of all building overlay group.</summary>
    [SerializeField] public List<RectTransform> buildingOverlays = new();

    /// <summary>Asks if there's a need to create a new overlay group.</summary>
    bool buildGridFilled = false;
    #endregion

    /// <summary>
    /// Moves placing grid and if needed fill it.
    /// </summary>
    /// <param name="building">Buidling to move with.</param>
    public void MovePlaceOverlay(Building building)
    {
        if (overlayParent.gameObject.activeSelf)
        {
            GridPos gp = MyGrid.Rotate(building.blueprint.moveBy, building.transform.rotation.eulerAngles.y);
            overlayParent.anchoredPosition = new(building.transform.position.x - gp.x, building.transform.position.z - gp.z);
            overlayParent.localRotation = Quaternion.Euler(180, 0, building.transform.rotation.eulerAngles.y);
            if (buildGridFilled == false)
            {
                foreach (NeededGridItem item in building.blueprint.itemList)
                {
                    RectTransform tile = Instantiate(overlayTile, overlayParent).GetComponent<RectTransform>();
                    tile.anchoredPosition = new(item.pos.x, item.pos.z);
                    tile.localRotation = Quaternion.Euler(0, 0, 0);
                    if (item.itemType == GridItemType.Anchor)
                        tile.GetComponent<Image>().color = Color.yellow;
                    else if (item.itemType == GridItemType.Entrance)
                        tile.GetComponent<Image>().color = Color.grey;
                }
            }
            buildGridFilled = true;
        }
    }

    /// <summary>Clears all tiles from the <see cref="overlayParent"/>.</summary>
    public void DestroyBuilingTiles()
    {
        for (int i = overlayParent.childCount - 1; i >= 0; i--)
        {
            Destroy(overlayParent.GetChild(i).gameObject);
        }
        buildGridFilled = false;
    }

    /// <summary>
    /// Creates an overlay group for entry points.
    /// </summary>
    /// <param name="gridPos">Anchor position.</param>
    /// <param name="id">Id of the building.</param>
    public void AddBuildingOverlay(GridPos gridPos, int id)
    {
        RectTransform t = Instantiate(groupPrefab, entryPointParent).GetComponent<RectTransform>();
        t.anchoredPosition = new(gridPos.x, -gridPos.z);
        t.name = id.ToString();
        buildingOverlays.Add(t);
    }

    /// <summary>
    /// Adds an entrypoint overaly tile, use child index only with user interactions(previously created, that just need to be moved).
    /// </summary>
    /// <param name="gridPos">Position for the entrypoint.</param>
    /// <param name="childIndex">Index for recycling overlay tiles.</param>
    public void Add(GridPos gridPos, int childIndex = -1)
    {
        RectTransform rect;
        if (childIndex == -1)
        {
            rect = Instantiate(overlayTile, buildingOverlays[^1]).GetComponent<RectTransform>();
            rect.anchoredPosition = new(gridPos.x, gridPos.z);
        }
        else
        {
            rect = overlayParent.GetChild(childIndex).GetComponent<RectTransform>();
            rect.transform.SetParent(buildingOverlays[^1]);
        }
        rect.gameObject.layer = 5;
        rect.GetComponent<Image>().color = new(0.5f, 0.5f, 0.5f, 0.25f);
    }

    /// <summary>
    /// Removes entry points of a <see cref="Building"/>.
    /// </summary>
    /// <param name="id">Id of the removed building.</param>
    /// <param name="y">Level of the tile.</param>
    public void Remove(int id, int y)
    {
        int i = buildingOverlays.FindIndex(q => q.name == id.ToString());
        if (i > -1)
        {
            Destroy(buildingOverlays[i].gameObject);
            foreach (GridPos gp in buildingOverlays[i].GetComponentsInChildren<Transform>().Skip(1).Select(q => new GridPos(q.transform.position)))
            {
                ClickableObject clickable = MyGrid.GetGridItem(new(gp.x, y, gp.z));
                if (clickable is Road)
                {
                    ((Road)clickable).entryPoints.Remove(id);
                }
            }
            buildingOverlays.RemoveAt(i);
        }
    }

    /// <summary>
    /// Toggles entry point visibility, when a building is built over the point.
    /// </summary>
    /// <param name="r">Road with the tile.</param>
    public void ToggleEntryPoints(Road r)
    {
        if (r)
            foreach (int id in r.entryPoints)
            {
                RectTransform rect = buildingOverlays.First(q => q.name == id.ToString());
                for (int i = 0; i < rect.transform.childCount; i++)
                {
                    GameObject tileObject = rect.GetChild(i).gameObject;
                    if (r.GetPos().Equals(new GridPos(tileObject.transform.position)))
                    {
                        tileObject.SetActive(false);
                        break;
                    }
                }
            }
    }
}
