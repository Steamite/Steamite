using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ProductionButton : Button
{
    [UxmlAttribute]
    public bool enabled;

    
    public void Init()
    {
        
    }

    public void UpdateButtonState(float a, float b)
    {
        //transform.GetChild(0).GetComponent<Image>().fillAmount = a > b ? 0.01f : a / b;
    }

    void ToggleButton()
    {
        /*ProductionBuilding pD = transform.parent.parent.GetChild(0).GetComponent<WorkerAssign>()._building.GetComponent<ProductionBuilding>();
        bool stoped = pD.StopProduction();
        transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(!stoped);
        transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(stoped);*/
    }
}
