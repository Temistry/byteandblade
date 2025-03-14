using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : UnityEngine.Object
{
    private static T instance;
    public virtual void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static T GetInstance()
    {

        return instance;
    }
}
