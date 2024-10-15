using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeInputField : MonoBehaviour
{
    public void UpdateTradeCost()
    {
        TMP_InputField inputField = gameObject.GetComponent<TMP_InputField>();

        if (inputField.text.Length > 0 && inputField.text[0] == '0')
            inputField.text = inputField.text.Remove(0);
        else
            MyGrid.canvasManager.tradeWindow.tradeInfo.UpdateTradeText();
    }
}
