using UnityEngine;
using Newtonsoft.Json;

namespace TRPG
{
    public static class SaveLoadUtil
    {
        public static void Save(string key, object value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                string json = JsonConvert.SerializeObject(value);
                PlayerPrefs.SetString(key, json);
            }
        }

        public static T Load<T>(string key) where T : class
        {
            if (!string.IsNullOrEmpty(key))
            {
                string json = PlayerPrefs.GetString(key);
                Debug.Log($"Data received: {json}");
                T data = JsonConvert.DeserializeObject<T>(json);    
                return (T)data;
            }
            return null;
        }

        public static string Get(string key)
        {
            return PlayerPrefs.GetString(key);
        }
    }

    public static class SaveKeys
    {
        public static string UserData = "userData";
    }
}