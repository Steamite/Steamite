using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoButtons : MonoBehaviour
{
    public int id;

    public void ManageWorkers() // assigns or unassigns worker
    {
        //transform.parent.parent.parent.parent.GetComponent<WorkerAssign>().ManageHuman(id);
    }

    public void ManageStorage(bool status)
    {
        gameObject.GetComponent<Button>().interactable = false;
        transform.parent.GetChild(status ? 2 : 1).GetComponent<Button>().interactable = true;
        Storage storage = transform.parent.parent.parent.GetComponent<StorageAssign>().building.GetComponent<Storage>();
        storage.canStore[id] = status;
    }    
}
