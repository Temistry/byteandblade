using UnityEngine;
using System.Collections;
public class UI_LoadingScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(StartLoading());
    }

    IEnumerator StartLoading()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
