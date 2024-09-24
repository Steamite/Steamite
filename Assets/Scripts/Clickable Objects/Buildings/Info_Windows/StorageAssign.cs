using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageAssign : MonoBehaviour
{
    public Building building;
    public void UpdateAmmounts()
    {
        int j = transform.GetChild(0).childCount - 1; // get number of resource items
        Transform tran = transform.GetChild(0);
        Resource res = building.localRes.stored;
        for(int i = 0; i < j; i++) // for each resource item in content
        {
            int x = res.type.IndexOf((ResourceType)i);
            int id = x == -1 ? i : (int)res.type[x];
            if (id >= tran.childCount || x >= tran.childCount)
                break;
            tran.GetChild(id).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = x == -1 ? "0": res.ammount[x].ToString();
        }
        tran.GetChild(j).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = $"{building.localRes.stored.ammount.Sum()}/{building.localRes.stored.capacity}";
    }
    public void SetStorageButton(List<bool> canStore, Transform button)
    {
        button = button.GetChild(1).GetChild(0);
        int i = 0;
        foreach (bool active in canStore)
        {
            button.GetChild(i).GetChild(1).gameObject.GetComponent<Button>().interactable = !active;
            button.GetChild(i).GetChild(2).gameObject.GetComponent<Button>().interactable = active;
            i++;
        }
    }
}