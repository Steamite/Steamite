using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProductionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    
    // Start is called before the first frame update

    public void UpdateButtonState(float a, float b)
    {
        transform.GetChild(0).GetComponent<Image>().fillAmount = a > b ? 0.01f : a / b;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.GetChild(1).GetComponent<Image>().color = new(1, 0.54f, 0, 0.2f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        transform.GetChild(1).GetComponent<Image>().color = new(0, 0, 0, 0);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //TODO
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ProductionBuilding pD = transform.parent.parent.GetChild(0).GetComponent<WorkerAssign>()._building.GetComponent<ProductionBuilding>();
        bool stoped = pD.StopProduction();
        transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(!stoped);
        transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(stoped);
    }
}
