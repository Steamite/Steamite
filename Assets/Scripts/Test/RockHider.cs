using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RockHider : MonoBehaviour
{
    Material material;
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        StartCoroutine(SwitchMode());
    }

    public IEnumerator SwitchMode()
    {
        int phase = 0;
        while (true)
        {
            switch (phase)
            {
                case 0:
                    material.SetFloat("_isHidden", 1);
                    material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    break;
                case 1:
                    material.SetColor("_EmissionColor", new (1,1,1,1));
                    break;
                case 2:
                    material.SetColor("_EmissionColor", new());
                    material.SetFloat("_isHidden", 0);
                    material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    phase = -1;
                    break;
            }
            phase++;
            yield return new WaitForSeconds(2);
        }
    }

}
