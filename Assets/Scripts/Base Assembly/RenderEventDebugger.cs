using UnityEngine;
using UnityEngine.UIElements;

public class RenderEventDebugger : MonoBehaviour, IDebugPanelChangeReceiver
{
    [SerializeField] PanelSettings m_PanelSettings;
    [SerializeField] bool debug;


    void Awake()
    {
        if(debug)
            m_PanelSettings.SetPanelChangeReceiver(this);
    }
    
    void OnDestroy()
    {
        m_PanelSettings.SetPanelChangeReceiver(null);
    }
    
    public void OnVisualElementChange(VisualElement element, VersionChangeType changeType)
    {
        Debug.Log($"{element.name} {changeType}");
    }
}
