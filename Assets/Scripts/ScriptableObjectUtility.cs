// Cloning method in a utility class

using UnityEngine;

public static class ScriptableObjectUtility
{
    public static T Clone<T>(T source) where T : ScriptableObject
    {
        T clone = ScriptableObject.Instantiate(source);
        return clone;
    }
}
