using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Trading : MonoBehaviour
{
    TradeMap map;
    [SerializeField] TradeHolder tradeHolder;
    private void Awake()
    {
        map = GetComponent<UIDocument>().rootVisualElement.Q<TradeMap>("Map");
        map.FillTradeLocations(tradeHolder);
    }
}
