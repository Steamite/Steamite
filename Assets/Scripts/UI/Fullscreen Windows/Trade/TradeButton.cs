using UnityEngine;

public class TradeButton : MonoBehaviour
{
    [SerializeField] int tradeLocationIndex = -1;

    public void OpenInfo()
    {
        MyGrid.canvasManager.trade.SelectButton(Trade.SelectedCateg.Trade, tradeLocationIndex);
    }
}