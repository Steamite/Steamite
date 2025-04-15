using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocalInfoWindow : UIBehaviour
{
    RectTransform rectTransform;
    Canvas c;
    GridPos gridPos = new();
    Status status = Status.notSet;
    enum Status{
        notSet,
        set,
        buildings,
        expeditions,
        levels
    }

    public void SetUp()
    {
        rectTransform = (RectTransform)transform;
        c = transform.parent.GetComponent<Canvas>();
        status = Status.set;
    }
    protected override void OnRectTransformDimensionsChange()
    {
        float canvasHeight;
        switch (status)
        {
            case Status.buildings:
                canvasHeight = c.renderingDisplaySize.y;
                rectTransform.anchoredPosition = new(
                    gridPos.x, 
                    (gridPos.z + ((canvasHeight / 13) + (rectTransform.rect.height / 2))) * c.scaleFactor);
                break;
            case Status.expeditions:
                canvasHeight = c.renderingDisplaySize.y;
                rectTransform.anchoredPosition = new(
                    gridPos.x + (rectTransform.rect.width / 2) + 13 * c.scaleFactor, 
                    gridPos.z + (rectTransform.rect.height / 2) + 13 * c.scaleFactor);
                break;
            default:
                return;
        }
        transform.gameObject.SetActive(true);
    }
    //-----------Display options-------------//
    public void DisplayInfo(Building prefab, Vector3 pos)
    {
        // triggers on button enter, works after setingUp
        if(status != Status.notSet)
        {
            status = Status.buildings;
            gridPos = new(pos.x, pos.y);
            print(prefab.objectName);
            transform.GetChild(0).GetComponent<TMP_Text>().text = prefab.objectName;
            transform.GetChild(1).GetComponent<TMP_Text>().text = string.Join('\n', prefab.GetInfoText());
            transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
            gameObject.SetActive(true);
        }
    }

    public void DisplayInfo(TradeConvoy expedition, Vector3 pos)
    {
        /*// triggers on button enter, works after setingUp
        if (status != Status.notSet)
        {
            status = Status.expeditions;
            gridPos = new(pos.x, pos.y);
            print(gridPos);
            transform.GetChild(0).GetComponent<TMP_Text>().text = UIRefs.trade.tradeLocations[expedition.tradeLocation].name;
            transform.GetChild(1).GetComponent<TMP_Text>().text = expedition.ToString();
            transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
            gameObject.SetActive(true);
        }*/
    }

    //public void DisplayInfo()

    void OnApplicationPause()
    {
        HideInfo();
    }

    public void HideInfo()
    {
        gameObject.SetActive(false);
    }
}
