using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildCategoryButton : MonoBehaviour
{
    // selects category that is supposed to be opened
    public void ToggleCategory(int toggleCateg)
    {
        Transform categories = transform.parent.parent.parent.GetChild(1);
        for (int i = 0; i < categories.childCount; i++)
        {
            GameObject category = categories.GetChild(i).GetChild(0).gameObject;
            if (i == toggleCateg)
                category.gameObject.SetActive(!category.activeSelf);
            else
                category.gameObject.SetActive(false);
        }
    }
}
