using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class KeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public KeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public static void Serialize(Dictionary<TKey, TValue> map, List<KeyValuePair<TKey, TValue>> list)
    {
        if (map == null || list == null)
        {
            Debug.LogError($"map {map == null} : list {list == null}");
            return;
        }

        list.Clear();
        var keys = map.Keys.ToList();

        for (var i = 0; i < keys.Count; i++)
        {
            list.Add(new KeyValuePair<TKey, TValue>(keys[i], map[keys[i]]));
        }
    }
    public static void Deserialize(Dictionary<TKey, TValue> map, List<KeyValuePair<TKey, TValue>> list)
    {
        if (map == null || list == null)
        {
            Debug.LogError($"map {map} : list {list}");
            return;
        }
        map.Clear();
        for (var i = 0; i < list.Count; i++)
        {
            map[list[i].Key] = list[i].Value;
        }
    }
}