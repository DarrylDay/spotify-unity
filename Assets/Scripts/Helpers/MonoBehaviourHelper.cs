using UnityEngine;
using System.Collections;
using System;

public class MonoBehaviourHelper : Singleton<MonoBehaviourHelper>
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        var go = new GameObject("MonoBehaviour Helper");
        go.AddComponent<MonoBehaviourHelper>();
        DontDestroyOnLoad(go);
    }

    public static event Action OnApplicationQuitEvent;

    public static Coroutine RunCoroutine(IEnumerator enumerator)
    {
        return Instance.StartCoroutine(enumerator);
    }

    public static void AbortCoroutine(Coroutine coroutine)
    {
        Instance.StopCoroutine(coroutine);
    }

    void OnApplicationQuit()
    {
        OnApplicationQuitEvent?.Invoke();
    }
}

