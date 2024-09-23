using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance { get; private set; }

    private void Awake()
    {
        Instance = this; 
    }

    public Coroutine RunCoroutineLocal(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public void StopCoroutineLocal(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }
}
