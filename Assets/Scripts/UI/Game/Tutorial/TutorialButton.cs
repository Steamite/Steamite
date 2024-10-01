using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    [SerializeField] string body;

    string ConvertLines()
    {
        int i = 0;
        char[] bodyWithLines = body.ToCharArray();
        char lastChar = ' ';
        char currentChar = ' ';
        while(i < bodyWithLines.Length-1)
        {
            lastChar = currentChar;
            currentChar = bodyWithLines[i];
            if (lastChar == '\\' && currentChar == 'n')
            {
                bodyWithLines[i - 1] = ' ';
                bodyWithLines[i] = '\n';
            }
            i++;
        }

        return bodyWithLines.ArrayToString();
    }
    public void OpenTutorialWindow(TutorialMenu tutorialMenu)
    {
        if (!tutorialMenu.gameObject.activeSelf)
        {
            Camera.main.GetComponent<PhysicsRaycaster>().enabled = false;
            Camera.main.GetComponent<Physics2DRaycaster>().enabled = false;
            tutorialMenu.gameObject.SetActive(true);
        }
        else
            tutorialMenu.lastButton.interactable = true;

        tutorialMenu.header.text = name;
        tutorialMenu.body.text = ConvertLines();
        tutorialMenu.lastButton = GetComponent<Button>();
        tutorialMenu.lastButton.interactable = false;
        tutorialMenu.gameObject.SetActive(true);
    }
    public void CloseTutorialWindow(TutorialMenu tutorialMenu) 
    {
        tutorialMenu.lastButton.interactable = true;
        tutorialMenu.lastButton = null;
        tutorialMenu.gameObject.SetActive(false);
        Camera.main.GetComponent<PhysicsRaycaster>().enabled = true;
        Camera.main.GetComponent<Physics2DRaycaster>().enabled = true;
    }
}
