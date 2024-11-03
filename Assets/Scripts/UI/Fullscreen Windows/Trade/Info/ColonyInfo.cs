using System;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class ColonyInfo : MonoBehaviour
{
    public void Hide(Trade trade)
    {
        gameObject.SetActive(false);
        trade.window.transform.GetChild(1).GetChild(trade.window.transform.GetChild(1).transform.childCount - 1).GetChild(0).GetComponent<Animator>().SetTrigger("unselected");
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
