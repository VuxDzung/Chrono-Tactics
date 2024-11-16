using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
public class SessionManager
{
    private static Dictionary<string, object> attributeCache = new Dictionary<string, object>();

    public static void SetAttribute(string attribute, object value)
    {
        attributeCache.Add(attribute, value);
    }

    public static void RemoveAttribute(string attribute)
    {
        attributeCache.Remove(attribute);
    }

    public static T GetAttribute<T>(string attribute) where T : class
    {
        if (attributeCache.ContainsKey(attribute))
            return (T)attributeCache[attribute];
        else
            return null;
    }
}