using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoButtons : MonoBehaviour
{
    public int id;
    public void CloseInfoMenu() // closes the info window
    {
        transform.parent.parent.gameObject.SetActive(false);
        GridTiles g = GameObject.Find("Grid").GetComponent<GridTiles>();
        g.DeselectObjects();
    }
    public void SwitchViews(bool state) // changes assign to unassign and back
    {
        GameObject.Find("Info Window").GetComponent<InfoWindow>().SetAssignButton(state, transform.parent);
    }
    public void ManageWorkers() // assigns or unassigns worker
    {
        transform.parent.parent.parent.parent.GetComponent<WorkerAssign>().ManageHuman(id);
    }
    public void ManageStorage(bool status)
    {
        gameObject.GetComponent<Button>().interactable = false;
        transform.parent.GetChild(status ? 2 : 1).GetComponent<Button>().interactable = true;
        Storage storage = transform.parent.parent.parent.GetComponent<StorageAssign>().building.GetComponent<Storage>();
        storage.canStore[id] = status;
    }
    public void ChangeTime()
    {
        bool b = id == 1 ? true : false;
        id = b ? 0 : 1;
        if (b)
        {
            DayTime.day?.Invoke();
        }
        else
        {
            DayTime.night?.Invoke();
        }
        transform.GetChild(0).GetComponent<TMP_Text>().text = b ? "Day": "Night";
    }
    
}
