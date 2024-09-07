using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOverlay : MonoBehaviour
{
    [SerializeField] Image overlayTile;
    [SerializeField] public RectTransform overlayParent;

    bool buildGridCreated = false;
    public void CreateBuildGrid(Building building)
    {
        if (overlayParent.gameObject.activeSelf)
        {
            GridPos gp = MyGrid.CheckRotation(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y);
            overlayParent.anchoredPosition = new(building.transform.position.x - gp.x + 0.5f, building.transform.position.z - gp.z + 0.5f);
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
}
