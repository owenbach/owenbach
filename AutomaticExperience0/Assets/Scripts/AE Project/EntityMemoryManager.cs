using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public static class EntityMemoryManager
{
    static bool isInitialized = false;
    static List<string> unneccessaryTypes = new List<string>();
    public static List<string> UnneccessaryTypes { get => unneccessaryTypes; set => unneccessaryTypes = value; }
    public static bool IsInitialized { get => isInitialized; set => isInitialized = value; }

    public static void Initialize()
    {
        if (isInitialized) return;
        string unnecessaryFunctionsLog = "Generated Functions To Ignore: ";
        GameObject das = new GameObject();
        das.AddComponent<VacentScript>();
        foreach(var type in das.GetType().GetMethods())
        {
            unneccessaryTypes.Add(type.Name);
            unnecessaryFunctionsLog += "\n - " + type.Name;
        }
        foreach (var type in das.GetComponent<VacentScript>().GetType().GetMethods())
        {
            if (unneccessaryTypes.Contains(type.Name)) continue;
            unneccessaryTypes.Add(type.Name);
            unnecessaryFunctionsLog += "\n - " + type.Name;
        }
        isInitialized = true;
        GameObject.Destroy(das);
        Debug.Log(unnecessaryFunctionsLog);
    }
}
