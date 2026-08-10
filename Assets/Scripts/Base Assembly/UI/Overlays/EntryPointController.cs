using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EntryPointController : MonoBehaviour, IBeforeLoad
{
    /// <summary>
    /// Creates an overlay group for entry points.
    /// </summary>
    /// <param name="gridPos">Anchor position.</param>
    /// <param name="id">Id of the building.</param>
    public BuildingEntryPoints AddBuildingEntryPointGroup(GridPos gridPos, Building building)
    {
        GameObject groupObject = new("GroupOverlay", typeof(RectTransform))
        {
            layer = LayerMask.NameToLayer("Overlays"),
        };
        groupObject.transform.SetParent(transform.GetChild(gridPos.y));


        RectTransform rect = groupObject.GetComponent<RectTransform>();
        rect.anchoredPosition3D = new(gridPos.x, gridPos.z, 0);
        rect.anchorMin = new(0, 0);
        rect.anchorMax = new(0, 0);
        rect.localRotation = Quaternion.Euler(0, 0, 0);

        BuildingEntryPoints points = new(new(), rect);
        building.entryPoints = points;
        return points;
    }


    /// <summary>
    /// Adds an entrypoint overaly tile, use child index only with user interactions(previously created, that just need to be moved).
    /// </summary>
    /// <param name="localPosition">Position for the entrypoint.</param>
    /// <param name="childIndex">Index for recycling overlay tiles.</param>
    public void AddNew(GridPos localPosition, RectTransform overlay, bool road = true)
    {
        Image image = OverlayUtils.CreateTile(
            localPosition, 
            overlay, 
            localPosition.ToString());

        image.color = road 
            ? new(0.5f, 0.5f, 0.5f, 0.25f) 
            : new(0.1f, 0.1f, 0.1f, 0.25f);
        
        Add(overlay, image.rectTransform);
    }

    public void AddFromIndicator(
        RectTransform overlay,
        int childIndex)
    {
        RectTransform rect = SceneRefs.Overlays.blueprintIndicator.GetTile(childIndex);//.GetChild(childIndex).GetComponent<RectTransform>();

        rect.SetParent(overlay);
        Add(overlay, rect);
    }

    void Add(RectTransform overlay, RectTransform imageRect)
    {
        imageRect.gameObject.layer = 5;
    }

    /// <summary>
    /// Removes entry points of a <see cref="Building"/>.
    /// </summary>
    /// <param name="id">Id of the removed building.</param>
    /// <param name="y">Level of the tile.</param>
    public void Remove(Building building)
    {
        Destroy(building.entryPoints.Group.gameObject);
        foreach (GridPos gp in building.entryPoints.AllTiles)
        {
            if (MyGrid.GetGridItem(gp) is Road road)
            {
                road.entryPoints.Remove(building);
            }
        }
    }

    /// <summary>
    /// Toggles entry point visibility, when a building is built over the point.
    /// </summary>
    /// <param name="r">Road with the tile.</param>
    public void ToggleEntryPoints(Road r, bool newState)
    {
        if (!r)
            return;
        GridPos pos = r.GetPos();
        foreach (Building building in r.entryPoints)
        {
            building.entryPoints.TogglePoint(pos, newState);
        }
    }


    public RectTransform Create(Building building)
    {
        return OverlayUtils.CreateGroup(
            transform.GetChild(building.GetPos().y) as RectTransform,
            building.GetPos(),
            building.Name);
    }

    Task IBeforeLoad.BeforeInit()
    {
        RectTransform rect = transform as RectTransform;
        rect.pivot = new(0, 0);
        rect.anchoredPosition = new(0, 0);

        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            RectTransform level = new GameObject(
                i.ToString(), 
                typeof(RectTransform))
                .GetComponent<RectTransform>();
            level.SetParent(transform, false);

            level.anchoredPosition3D = new(0, 0, -(i*ClickableObjectFactory.LEVEL_HEIGHT) -0.501f);
            level.anchorMin = new(0, 0);
            level.anchorMax = new(0, 0);
            level.sizeDelta = new(0, 0);

            level.gameObject.SetActive(false);
        }
        MyGrid.AddToGridChange(ToggleEntryPointLevel);

        return Task.CompletedTask;
    }

    private void ToggleEntryPointLevel(int old, int newLevel)
    {
        transform.GetChild(old).gameObject.SetActive(false);
        transform.GetChild(newLevel).gameObject.SetActive(true);
    }
}
