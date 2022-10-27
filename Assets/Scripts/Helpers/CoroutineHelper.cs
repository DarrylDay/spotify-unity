using UnityEngine;
using System.Collections;

public class CoroutineHelper : Singleton<CoroutineHelper>
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        var go = new GameObject("Coroutine Helper");
        go.AddComponent<CoroutineHelper>();
        DontDestroyOnLoad(go);
    }

    public static Coroutine Run(IEnumerator enumerator)
    {
        return Instance.StartCoroutine(enumerator);
    }

    public static void Stop(Coroutine coroutine)
    {
        Instance.StopCoroutine(coroutine);
    }
}

