using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public void ChangeLevel(int i)
    {
        GroundLevel groundLevel = gameObject.GetComponent<GroundLevel>();
        transform.GetChild(i).GetComponent<Button>().interactable = true;
        //GroundLevel.SetLevel(i);
    }
}
