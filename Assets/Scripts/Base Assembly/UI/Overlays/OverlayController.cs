using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

struct XY
{
    public int x;
    public int y;

    public XY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class OverlayController : MonoBehaviour, IAfterLoad
{
    List<XY> positions;
    Image[,] images;
    [SerializeField] Gradient integrityColor;
    [SerializeField] float maxIntegrity;
    [SerializeField] Material overlayMaterial;
    //[SerializeField] Vector2 integrityRange;

    [SerializeField] InputAction toggleIntegrityOverlay;
    bool integrity;

    public void AfterInit()
    {
        RectTransform rect = transform as RectTransform;
        int grid = MyGrid.GridSize;
        images = new Image[grid, grid];
        positions = new();
        GridPos pos = new();

        for (int x = 0; x < grid; x++)
        {
            pos.x = x;
            for (int y = 0; y < grid; y++)
            {
                pos.z = -y;
                Image image = OverlayUtils.CreateTile(
                    pos, 
                    rect, 
                    pos.ToString());
                image.gameObject.SetActive(false);
                image.material = overlayMaterial;
                image.maskable = false;
                images[x, y] = image;
            }
        }

        integrity = false;
        toggleIntegrityOverlay.Enable();
        toggleIntegrityOverlay.performed += (e) =>
        {
            if (!integrity)
                IntegrityOverlay();
            else
                ClearTiles();
            integrity = !integrity;
        };
    }

    void ClearTiles()
    {
        foreach (var item in images)
        {
            item.gameObject.SetActive(false);
        }
        /*
        foreach (var item in positions)
        {
            images[item.x, item.y].gameObject.SetActive(false);
        }*/
        positions.Clear();
    }

    void ClearAllTiles()
    {
        foreach (var item in images)
        {
            item.gameObject.SetActive(false);
        }
        positions.Clear();
    }


    public void MarkTiles(IEnumerable<GridPos> pos, Color color)
    {
        ClearTiles();

        foreach (var item in pos)
        {
            XY xy = new((int) item.x, (int) item.z);

            images[xy.x, xy.y].gameObject.SetActive(true);
            images[xy.x, xy.y].color = color;

            positions.Add(xy);
        }
    }

    public void IntegrityOverlay()
    {
        ClearTiles();

        int size = MyGrid.GridSize;
        var grid = MyGrid.GetGridTilesCurrentLevel();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                images[i, j].gameObject.SetActive(true);

                images[i, j].color = integrityColor.Evaluate(grid[i, j].Stability / maxIntegrity);
            }
        }
    }

    /*public void CreateTileOverlay(IEnumerable<GridPos> positions)
    {
        OverlayGroup = new("GroupOverlay", typeof(RectTransform));
        OverlayGroup.layer = LayerMask.NameToLayer("Overlays");
        OverlayGroup.transform.SetParent(transform.GetChild(0));

        RectTransform rect = OverlayGroup.GetComponent<RectTransform>();
        rect.anchoredPosition3D = new(0, 0, 0);
        rect.anchorMin = new(0, 0);
        rect.anchorMax = new(0, 0);
        rect.localRotation = Quaternion.Euler(180, 0, 0);


        foreach (GridPos pos in positions)
        {
            RectTransform tile = Instantiate(overlayTile, OverlayGroup.transform).GetComponent<RectTransform>();
            tile.anchoredPosition = new(pos.x, -pos.z);
            tile.localRotation = Quaternion.Euler(0, 0, 0);
            tile.GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.25f);
        }
    }*/
}