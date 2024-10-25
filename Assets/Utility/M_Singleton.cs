using UnityEngine;

namespace Utils
{
    public class M_Singleton<T> : MonoBehaviour where T : Component
    {
        [SerializeField]
        protected bool dontDestroyOnLoad = true;

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

        protected virtual void Awake()
        {
            if (dontDestroyOnLoad)
            {
                if (instance == null)
                {
                    instance = this as T;
                    DontDestroyOnLoad(instance);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}