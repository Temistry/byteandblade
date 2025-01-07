using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : UnityEngine.Object
{
    private static T instance;
    public static T GetInstance()
    {
        if(instance == null)
        {
            instance = Object.FindFirstObjectByType<T>();
            DontDestroyOnLoad(instance);
        }
        return instance;
    }
}
