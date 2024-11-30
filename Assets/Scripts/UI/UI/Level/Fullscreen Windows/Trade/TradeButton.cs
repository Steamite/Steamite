using UnityEngine;

public class TradeButton : MonoBehaviour
{
    [SerializeField] int tradeLocationIndex = -1;

    public void OpenInfo()
    {
        UIRefs.trade.SelectButton(Trade.SelectedCateg.Trade, tradeLocationIndex);
    }
}