using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class PerformanceSaver : MonoBehaviour
{

    double[] times;
    public int x = 0;
    bool StartAtX;
    [SerializeField] InputAction save;
    [SerializeField] InputAction flush;
    private void Awake()
    {
        times = new double[2000];
        x = 0;
        StartAtX = false;

    }

    private void OnEnable()
    {
        save.Enable();
        flush.Enable();

        save.performed += Save_performed;
        flush.performed += Flush_performed;
        GetComponent<FPSCounter>().onUpdate += OnUpdate;
    }

    private void Save_performed(InputAction.CallbackContext obj) => SaveToFile();
    

    private void OnDisable()
    {
        save.Disable();
        flush.Disable();

        save.performed -= Save_performed;
        flush.performed -= Flush_performed;

        GetComponent<FPSCounter>().onUpdate -= OnUpdate;
    }


    private void Flush_performed(InputAction.CallbackContext obj)
    {
        for (int i = 0; i < times.Length; i++)
        {
            times[i] = 0;
        }
        x = 0;
        StartAtX = false;
    }

    public void OnUpdate(double realWorkMs, double totalMs)
    {
        if (realWorkMs > 0.000f)
        {
            times[x] = realWorkMs;
            x++;
            if (x >= times.Length)
            {
                x = 0;
                StartAtX = true;
            }
        }
    }


    public void SaveToFile()
    {
        if (!Directory.Exists($"{Application.persistentDataPath}/benchmark"))
            Directory.CreateDirectory($"{Application.persistentDataPath}/benchmark/");
        using FileStream fs = new FileStream($"{Application.persistentDataPath}/benchmark/{GUID.Generate()}", FileMode.Create);
        using BinaryWriter bw = new BinaryWriter(fs);

        bw.Write(StartAtX ? (Int32)x : 0);
        bw.Write(StartAtX ? times.Length : (Int32)x);

        foreach (double value in times)
        {
            bw.Write(value);
        }
    }
}
