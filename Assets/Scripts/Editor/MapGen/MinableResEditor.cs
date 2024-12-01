using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MinableRes))]
public class MinableResEditor : Editor
{
    bool setDirty;
    public override void OnInspectorGUI()
    {
        setDirty = false;
        base.OnInspectorGUI();
        MinableRes minable = (MinableRes)target;
        EnforceListLenghts(minable);


        if (setDirty)
            EditorUtility.SetDirty(target);
    }

    void EnforceListLenghts(MinableRes minable)
    {
        minable.richness = EnforceListLength(minable.richness);
        minable.size = EnforceListLength(minable.size);
        minable.count = EnforceListLength(minable.count);
    }

    #region Enforcing Logic
    VeinParameter[] EnforceListLength(VeinParameter[] veinParameter)
    {
        if (veinParameter.Length != 5)
        {
            VeinParameter[] _veinParameter = new VeinParameter[5];
            for (int i = 0; i < 5; i++)
            {
                if (veinParameter.Length > i)
                {
                    _veinParameter[i] = veinParameter[i];
                    EnforceMinMax(_veinParameter[i]);
                }
                else
                {
                    _veinParameter[i] = new();
                }
            }
            setDirty = true;
            return _veinParameter;
        }
        else
        {
            for(int i = 0; i < 5; i++)
            {
                EnforceMinMax(veinParameter[i]);
            }
            return veinParameter;
        }
    }

    int[] EnforceListLength(int[] array)
    {
        if (array.Length != 3)
        {
            int[] _array = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (array.Length > i)
                    _array[i] = array[i];
                else
                    _array[i] = 5;
            }
            setDirty = true;
            return _array;
        }
        return array;
    }


    void EnforceMinMax(VeinParameter veinParameter)
    {
        veinParameter.min = EnforceListLength(veinParameter.min);
        veinParameter.max = EnforceListLength(veinParameter.max);

        for(int i = 0; i < 3; i++)
        {
            if (veinParameter.min[i] + i + 1 > veinParameter.max[i])
            {
                veinParameter.max[i] = veinParameter.min[i] + i + 1;
                setDirty = true;
            }
        }
    }

    #endregion
}
