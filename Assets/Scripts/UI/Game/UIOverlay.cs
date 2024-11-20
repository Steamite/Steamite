using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIOverlay : MonoBehaviour
{
    [SerializeField] Image overlayTile;
    [SerializeField] public RectTransform overlayParent;
    [SerializeField] public RectTransform entryPointParent;
    [SerializeField] public GameObject groupPrefab;
    [SerializeField] public List<RectTransform> buildingOverlays = new();

    bool buildGridCreated = false;
    public void CreateBuildGrid(Building building)
    {
        if (overlayParent.gameObject.activeSelf)
        {
            GridPos gp = MyGrid.Rotate(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y);
            overlayParent.anchoredPosition = new(building.transform.position.x - gp.x, building.transform.position.z - gp.z);
            overlayParent.localRotation = Quaternion.Euler(180, 0, building.transform.rotation.eulerAngles.y);
            if(buildGridCreated == false)
            {
                foreach (NeededGridItem item in building.build.blueprint.itemList)
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
            buildGridCreated = true;
        }
    }
    public void DeleteBuildGrid()
    {
        for (int i = overlayParent.childCount-1; i >= 0; i--)
        {
            Destroy(overlayParent.GetChild(i).gameObject);
        }
        buildGridCreated = false;
    }

    public void AddBuildingOverlay(GridPos gridPos, int id)
    {
        RectTransform t = Instantiate(groupPrefab, entryPointParent).GetComponent<RectTransform>();
        t.anchoredPosition = new(gridPos.x, -gridPos.z);
        t.name = id.ToString();
        buildingOverlays.Add(t);
    }

    public void Add(GridPos gridPos, int i)
    {
        RectTransform rect;
        if (i == -1)
        {
            rect = Instantiate(overlayTile, buildingOverlays[^1]).GetComponent<RectTransform>();
            rect.anchoredPosition = new(gridPos.x, gridPos.z);
        }
        else
        {
            rect = overlayParent.GetChild(i).GetComponent<RectTransform>();
            rect.transform.SetParent(buildingOverlays[^1]);
        }
        rect.gameObject.layer = 5;
        rect.GetComponent<Image>().color = new(0.5f, 0.5f, 0.5f, 0.25f);
    }

    public void Remove(int id)
    {
        int i = buildingOverlays.FindIndex(q => q.name == id.ToString());
        if (i > -1)
        {
            Destroy(buildingOverlays[i].gameObject);
            foreach(GridPos gp in buildingOverlays[i].GetComponentsInChildren<Transform>().Skip(1).Select(q=> new GridPos(q.transform.position)))
            {
                ClickableObject clickable = MyGrid.GetGridItem(gp);
                if (clickable is Road)
                {
                    ((Road)clickable).entryPoints.Remove(id);
                }
            }
            buildingOverlays.RemoveAt(i);
        }
    }

    // show/hide entry points
    public void ToggleEntryPoints(Road r)
    {
        if (r)
            foreach(int id in r.entryPoints)
            {
                RectTransform rect = buildingOverlays.First(q => q.name == id.ToString());
                rect.GetComponentsInChildren<Image>().FirstOrDefault(q => new GridPos(q.transform.position).Equals(new GridPos(r.transform.position))).gameObject.SetActive(false);
            }
    }

    internal UIOverlay SelectOverlay(int lIndex)
    {
        throw new NotImplementedException();
    }

}
