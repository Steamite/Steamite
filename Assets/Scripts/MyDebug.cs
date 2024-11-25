using System;
using TMPro;
using UnityEngine;

public class MyDebug : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    private void Update()
    {
        if(Input.GetKey(KeyCode.Semicolon)){
            bool active = !transform.GetChild(0).gameObject.activeSelf;
            transform.GetChild(0).gameObject.SetActive(active);
            if (active)
                inputField.text = "";
        }
    }


    public void OnEnd()
    {
        try
        {
            string[] s = inputField.text.Split(' ');

            if (s[0] == "money")
            {
                int x;
                if (int.TryParse(s[1], out x))
                {
                    MyRes.ManageMoney(x);
                    inputField.text = "";
                }
                else
                {
                    throw new Exception("Invalid integer");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
