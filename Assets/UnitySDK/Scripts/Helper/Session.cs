using System.Collections.Generic;

public class Session 
{
    private static Dictionary<string, object> sessionCache = new Dictionary<string, object>();

    public static void SetAttribute(string key, object value)
    {
        sessionCache[key] = value;  
    }

    public static bool RemoveAttribute(string key)
    {
        if (sessionCache.ContainsKey(key)) 
            return sessionCache.Remove(key);
        return false;
    }

    public static object GetAttribute(string key)
    {
        return sessionCache[key];
    }
}
