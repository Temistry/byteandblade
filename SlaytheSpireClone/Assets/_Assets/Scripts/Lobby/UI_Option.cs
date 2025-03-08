using UnityEngine;
using UnityEngine.UI;
using System;


public class UI_Option : MonoBehaviour
{
    public Button backButton;
    UI_MessageBox messageBox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backButton.onClick.AddListener(OnBack);

        messageBox = GetComponent<UI_MessageBox>();
        messageBox.SetMessage(LanguageManager.GetText("reset player data?"));
        
        
        Action eventOK = () => OnResetButton();
        messageBox.SetOnClickOkButton(eventOK);
    }

    private void OnBack()
    {
        gameObject.SetActive(false);
    }

    public void OnResetButton()
    {
        GameManager.GetInstance().ResetPlayerData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
