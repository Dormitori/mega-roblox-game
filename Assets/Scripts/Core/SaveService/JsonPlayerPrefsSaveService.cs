using UnityEngine;
using Newtonsoft.Json;

public class JsonPlayerPrefsSaveService : ISaveService
{
    public void Save<T>(string key, T value)
    {
        var json = JsonConvert.SerializeObject(value);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public T Load<T>(string key)
    {
        return JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key));
    }

    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }
}   
