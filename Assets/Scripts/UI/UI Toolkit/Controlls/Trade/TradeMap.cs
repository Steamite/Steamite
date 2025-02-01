using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

[UxmlElement]
public partial class TradeMap : Map
{
    [UxmlAttribute][Range(100, 200)] float elementSize;
    #region Textures
    Texture2D locationImg;
    [UxmlAttribute]
    Texture2D locationImage
    {
        get => locationImg;
        set
        {
            locationImg = value;
            if(mapElem != null)
            {
                foreach (VisualElement location in mapElem.Children())
                {
                    location.style.backgroundImage = value;
                }
            }
        }
    }
    #endregion

    List<GridPos> positions;


    public void FillTradeLocations(TradeHolder holder)
    {
        positions = new() { holder.startingLocations[0].pos };
        positions = positions.Union(holder.tradeLocations.Select(q => q.pos)).ToList();
        ToggleControls();
        
        VisualElement locationButton;
        for (int i = 0; i < positions.Count; i++)
        {
            locationButton = new();
            if (i == 0)
                locationButton.style.unityBackgroundImageTintColor = Color.blue;
            locationButton.style.backgroundImage = locationImg;
            locationButton.style.position = Position.Absolute;
            mapElem.Add(locationButton);
            RecalculateLocation(i);
        }

    }
    
    protected override bool ZoomMap(WheelEvent wheelEvent)
    {
        if (base.ZoomMap(wheelEvent))
        {
            for (int i = 0; i < mapElem.childCount; i++)
            {
                RecalculateLocation(i);
            }
            return true;
        }
        return false;
    }

    void RecalculateLocation(int index)
    {
        VisualElement location = mapElem.ElementAt(index);
        location.style.width = elementSize * zoom;
        location.style.height = elementSize * zoom;

        location.style.left = (positions[index].x * zoom) - (location.style.width.value.value / 2);
        location.style.top = (positions[index].z * zoom) - (location.style.height.value.value / 2);
    }
}
