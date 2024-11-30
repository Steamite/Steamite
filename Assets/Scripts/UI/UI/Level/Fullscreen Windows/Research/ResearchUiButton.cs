using System;
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
        Completed
    }

    //Variables
    public ResearchNode node;
    public List<Image> unlockedByLines;
    public List<Image> unlocksLines;

    private int unlockedPrevs;
    private ResearchBackend backend;
    [SerializeField]public Image borderFill;

    public ButtonState state;
    bool decresing;

    //Methods
    //Initializes the button
    public void Initialize(ResearchNode researchNode, List<ResearchNode> nodes)
    {
        unlockedByLines = new();
        unlocksLines = new();
        name = researchNode.name;
        node = researchNode;
        transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = name;

        if (node.researched)
        {
            Complete(true);
            return;
        }
        else if(node.gp.y == 0 || node.unlockedBy.All(q => nodes.Find(x=> x.id == q).researched))
        {
            state = ButtonState.Available;
            Recolor();
            if (node.researchTime == 0)
                Debug.LogError($"researchTime not set: {node.gp.y}, {name}");
            borderFill.fillAmount = node.currentTime / node.researchTime;
        }
        else
        {
            state = ButtonState.Unavailable;
        }
        ManageBuildButton();
    }

    void Recolor(bool doLines = false)
    {
        switch (state)
        {
            case ButtonState.Unavailable:
                transform.GetChild(0).GetComponent<Image>().color = Col(63, 66, 67);
                transform.GetChild(1).GetComponent<Image>().color = Col(86, 90, 91);
                transform.GetChild(2).GetComponent<Image>().color = Col(105, 108, 109);
                break;
            case ButtonState.Available:
                transform.GetChild(0).GetComponent<Image>().color = Col(158, 101, 38);
                transform.GetChild(1).GetComponent<Image>().color = Col(197, 144, 49);
                transform.GetChild(2).GetComponent<Image>().color = Col(210, 159, 69);
                break;
            case ButtonState.Completed:
                transform.GetChild(0).GetComponent<Image>().color = Col(19, 128, 95);
                transform.GetChild(1).GetComponent<Image>().color = Col(22, 159, 120);
                transform.GetChild(2).GetComponent<Image>().color = Col(71, 187, 139);
                Destroy(borderFill);
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
                foreach (Image image in unlockedByLines)
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

    public void OnClick()
    {
        UIRefs.research.ResearchButtonClick(this);
    }

    void ManageBuildButton()
    {
        Transform button = CanvasManager.buildMenu.GetChild(1).GetChild(node.buttonCategory).transform.GetChild(node.buildButton);
        switch (state)
        {
            case ButtonState.Unavailable:
                button.gameObject.SetActive(false);
                if (button.parent.GetComponentsInChildren<BuildButton>().Length == 0)
                    CanvasManager.buildMenu.GetChild(0).GetChild(button.parent.GetSiblingIndex()).gameObject.SetActive(false);
                break;
            case ButtonState.Available:
                button.gameObject.SetActive(true);
                button.GetComponent<Button>().interactable = false;
                CanvasManager.buildMenu.GetChild(0).GetChild(button.parent.GetSiblingIndex()).gameObject.SetActive(true);
                break;
            case ButtonState.Completed:
                button.gameObject.SetActive(true);
                button.GetComponent<Button>().interactable = true;
                CanvasManager.buildMenu.GetChild(0).GetChild(button.parent.GetSiblingIndex()).gameObject.SetActive(true);
                break;
        }
    }

    public void Complete(bool init = false)
    {
        state = ButtonState.Completed;
        node.researched = true;
        node.currentTime = node.researchTime;
        if (!init)
        {
            EndAnim(true);
            foreach (int i in node.unlocks)
            {
                int categ = transform.parent.parent.GetSiblingIndex();
                UIRefs.research.GetResearchUIButton(categ, i).Unlock(this);
            }
            borderFill.fillAmount = 1;
        }
        CanvasManager.ShowMessage($"Reseach finished: {node.name}");
        Recolor(true);
        UIRefs.research.UpdateInfoWindow(this);
        ManageBuildButton();
    }

    void Unlock(ResearchUIButton currentResearch)
    {
        if (node.unlockedBy.Contains(currentResearch.node.id))
        {
            unlockedPrevs++;
            if (unlockedPrevs == node.unlockedBy.Count)
            {
                state = ButtonState.Available;
                Recolor();
            }
            ManageBuildButton();
        }
    }

    internal void StartAnim()
    {
        decresing = false;
        transform.GetChild(0).GetComponent<Animator>().SetFloat("Speed", 1);
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("selected");
    }

    internal void EndAnim(bool smooth = false)
    {
        if (smooth && gameObject.activeInHierarchy)
            StartCoroutine(SmoothEnd());
        else
            transform.GetChild(0).GetComponent<Animator>().SetTrigger("unselected");
    }

    IEnumerator SmoothEnd()
    {
        decresing = true;
        Animator animator = transform.GetChild(0).GetComponent<Animator>();
        float f = animator.GetFloat("Speed");
        while(f > 0)
        {
            if (!decresing)
                yield break;
            f -= 0.1f;
            animator.SetFloat("Speed", f);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        animator.SetFloat("Speed", 0);
    }
}
