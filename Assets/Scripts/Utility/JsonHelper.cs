using Newtonsoft.Json.Linq;
using UnityEngine;
/// <summary>
/// Json to/from Array.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Get Array from JSON.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="json">Json String</param>
    /// <returns></returns>
    public static T[] FromJson<T>(string json)
    {
        
        JToken jToken = JToken.Parse(json);
        Wrapper<T> wrapper = jToken.ToObject<Wrapper<T>>();
        return wrapper.server;
    }
    /// <summary>
    /// Convery Array to JSON.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="array">Array[]</param>
    /// <returns></returns>
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.server = array;
        return JsonUtility.ToJson(wrapper);
    }
    /// <summary>
    /// Convert Array to JSON. Pretty Print
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="prettyPrint">Pretty Print Bool</param>
    /// <returns></returns>
    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.server = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }
    /// <summary>
    /// Wrapper
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] server;
    }
}