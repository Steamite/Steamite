using UnityEngine;

public class TradeButton : MonoBehaviour
{
    [SerializeField] int tradeLocationIndex = -1;

    public void OpenInfo()
    {
        MyGrid.canvasManager.tradeWindow.SelectButton(Trade.SelectedCateg.Trade, tradeLocationIndex);
    }
}