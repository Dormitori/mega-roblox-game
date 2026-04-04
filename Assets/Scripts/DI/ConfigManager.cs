using System.Collections.Generic;
using UnityEngine;

public class ConfigManager<T> where T : ScriptableObject
{
    public List<T> Configs { get; }

    public ConfigManager()
    {
        Configs = new List<T>(Resources.LoadAll<T>($"{typeof(T).Name}"));
        if (Configs.Count == 0)
            Debug.Log($"Не нашли конфиги, добавь их в папку Resources в папку {typeof(T).Name}");
    }
}