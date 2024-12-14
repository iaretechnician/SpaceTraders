using UnityEngine;

public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            string name = typeof(T).FullName;

            if (_instance == null)
                _instance = Resources.Load<T>(name);

            if (_instance == null)
                Debug.LogError("ERROR: " + name + " not found! Asset must be in the Resources folder!");
            return _instance;
        }
    }
}
