using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeInputField : MonoBehaviour
{
    //[SerializeField] bool is
    public void UpdateTradeCost()
    {
        TMP_InputField inputField = gameObject.GetComponent<TMP_InputField>();
        MyGrid.canvasManager.trade.tradeInfo.UpdateTradeText();
    }
}
