using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Provider
{
    private static readonly Dictionary<string, Component> providerDic = new Dictionary<string, Component>(1000);

    public static void Add<T>(GameObject obj) where T : Component
    {
        if (obj == null) return;

        providerDic.Add($"{obj.GetInstanceID()}_{typeof(T).Name}", obj.GetComponent<T>());
    }

    public static void Remove<T>(GameObject obj) where T : Component
    {
        if (obj == null) return;

        providerDic.Remove($"{obj.GetInstanceID()}_{typeof(T).Name}");
    }

    public static T Get<T>(GameObject obj) where T : Component
    {
        if(obj == null) return null;

        if (!providerDic.ContainsKey($"{obj.GetInstanceID()}_{typeof(T).Name}"))        
            Add<T>(obj);        

        return providerDic[$"{obj.GetInstanceID()}_{typeof(T).Name}"] as T;
    }
}
