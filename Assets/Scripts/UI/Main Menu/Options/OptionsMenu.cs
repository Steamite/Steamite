using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public Transform mainMenu;
    public void CloseOptions()
    {
        mainMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    
    
}
