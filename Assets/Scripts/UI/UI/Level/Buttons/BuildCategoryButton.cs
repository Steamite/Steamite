using TMPro;
using UnityEngine;

public class BuildCategoryButton : MonoBehaviour
{
    // selects category that is supposed to be opened
    public void ToggleCategory(int categoryIndex)
    {
        if(categoryIndex > -1)
        {
            Transform categories = transform.parent.GetChild(1);
            categories.gameObject.SetActive(true);
            for (int i = 0; i < categories.childCount; i++)
            {
                GameObject category = categories.GetChild(i).gameObject;
                if (i == categoryIndex)
                {
                    if (!category.gameObject.activeSelf)
                        category.gameObject.SetActive(true);
                    else
                    {
                        category.gameObject.SetActive(false);
                        categories.gameObject.SetActive(false);
                    }
                }
                else
                    category.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError($"Category button({name}) not inicialized!");
        }
    }
}
