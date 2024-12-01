using UnityEngine;

public class NetworkSingleton<T> : CoreNetworkBehaviour where T : CoreNetworkBehaviour
{
    // create a private reference to T instance
    private static T instance;
    public static T Singleton
    {
        get
        {
            // if instance is null
            if (instance == null)
            {
                // find the generic instance
                instance = FindObjectOfType<T>();

                // if it's null again create a new object
                // and attach the generic instance
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }
}
