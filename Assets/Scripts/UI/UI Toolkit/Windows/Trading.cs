using UnityEngine;
using UnityEngine.UIElements;

public class Trading : MonoBehaviour
{
    Map map;
    private void Awake()
    {
        map = GetComponent<UIDocument>().rootVisualElement.Q<Map>("Map");
        map.ToggleControls();
    }
}
