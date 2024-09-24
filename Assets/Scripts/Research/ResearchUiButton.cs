using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchUIButton : MonoBehaviour
{
    public enum ButtonState
    {
        Unavailable,
        Available,
        Reseaching,
        Completed
    }

    //Variables
    public ResearchNode node;
    public List<Image> unlockedByLines;
    public List<Image> unlocksLines;

    private ResearchUI UI;
    private ResearchBackend backend;

    public ButtonState state;

    //Methods
    //Initializes the button
    public void Initialize(string _name, ResearchNode researchNode, List<ResearchNode> nodes)
    {
        unlockedByLines = new();
        unlocksLines = new();
        name = _name;
        node = researchNode;
        transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = _name;


        if (node.researched)
        {
            state = ButtonState.Completed;
            Recolor();
        }
        else if(node.gp.level == 0 || node.unlockedBy.All(q => nodes.Find(x=> x.id == q).researched))
        {
            state = ButtonState.Available;
            Recolor();
        }
        else
        {
            state = ButtonState.Unavailable;
        }
    }

    void Recolor(bool doLines = false)
    {
        switch (state)
        {
            case ButtonState.Unavailable:
                gameObject.GetComponent<Image>().color = Col(63, 66, 67);
                transform.GetChild(0).GetComponent<Image>().color = Col(86, 90, 91);
                transform.GetChild(1).GetComponent<Image>().color = Col(105, 108, 109);
                break;
            case ButtonState.Available:
            case ButtonState.Reseaching:
                gameObject.GetComponent<Image>().color = Col(158, 101, 38);
                transform.GetChild(0).GetComponent<Image>().color = Col(197, 144, 49);
                transform.GetChild(1).GetComponent<Image>().color = Col(210, 159, 69);
                break;
            case ButtonState.Completed:
                gameObject.GetComponent<Image>().color = Col(19, 128, 95);
                transform.GetChild(0).GetComponent<Image>().color = Col(22, 159, 120);
                transform.GetChild(1).GetComponent<Image>().color = Col(71, 187, 139);
                break;
        }
        if(doLines)
            RecolorLines();
    }

    public void RecolorLines()
    {
        switch (state)
        {
            case ButtonState.Unavailable:
                foreach (Image image in unlocksLines)
                {
                    image.color = Col(105, 108, 109);
                }
                break;
            case ButtonState.Available:
            case ButtonState.Reseaching:
                foreach (Image image in unlocksLines)
                {
                    image.color = Col(210, 159, 69);
                }
                break;
            case ButtonState.Completed:
                foreach (Image image in unlockedByLines)
                {
                    image.color = Col(29, 176, 126);
                }
                foreach (Image image in unlocksLines)
                {
                    image.color = Col(210, 159, 69);
                }
                break;
        }
    }

    Color Col(float r, float g, float b)
    {
        return new(r / 255f, g / 255f, b / 255f, 1);
    }

    public void Initialise(ResearchUI UI_passed)
    {
        if (UI_passed == null) Debug.Log("UI is null in button");
        UI = UI_passed;
        backend = UI.gameObject.GetComponent<ResearchBackend>();
        backend.researches[node.id].button = gameObject.GetComponent<Button>();
    }

    public void OnClick()
    {
        node.researched = true;
        Recolor();
        //UI.OnResearchButtonClick(node.id, gameObject.GetComponent<Button>());
    }
}
