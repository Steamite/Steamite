using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchUiButton : MonoBehaviour
{
    //Variables
    public int research_id;
    private ResearchUI UI;
    private ResearchBackend backend;
    
    //Methods
    //Initializes the button
    public void Initialise(ResearchUI UI_passed)
    {
        if (UI_passed == null) Debug.Log("UI is null in button");
        UI = UI_passed;
        backend = UI.gameObject.GetComponent<ResearchBackend>();
        backend.researches[research_id].button = gameObject.GetComponent<Button>();
    }
    public void OnClick()
    {
        UI.OnResearchButtonClick(research_id, gameObject.GetComponent<Button>());
    }
}
