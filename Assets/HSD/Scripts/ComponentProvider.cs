using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentProvider
{
    private static readonly Dictionary<string, Component> comDic = new Dictionary<string, Component>();

    public static T Get<T>(GameObject obj) where T : Component
    {
        Add<T>(obj);

        return comDic[$"{obj.GetInstanceID()}_{typeof(T).Name}"] as T;
    }

    public static void Add<T>(GameObject obj) where T : Component
    {
        if (!comDic.ContainsKey($"{obj.GetInstanceID()}_{typeof(T).Name}"))
            comDic[$"{obj.GetInstanceID()}_{typeof(T).Name}"] = obj.GetComponent<T>();
    }

    public static void Remove<T>(GameObject obj) where T : Component
    {
        if (comDic.ContainsKey($"{obj.GetInstanceID()}_{typeof(T).Name}"))
            comDic.Remove($"{obj.GetInstanceID()}_{typeof(T).Name}");
    }
}
