using UnityEngine;
using System.Collections;
using TMPro;
public class UI_LoadingScreen : MonoBehaviour
{

    public TextMeshProUGUI loadingText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingText.text = LanguageManager.GetText("Loading");
        StartCoroutine(StartLoading());
    }

    IEnumerator StartLoading()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
