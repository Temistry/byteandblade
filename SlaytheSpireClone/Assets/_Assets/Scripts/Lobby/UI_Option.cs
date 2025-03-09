using UnityEngine;
using UnityEngine.UI;
using System;


public class UI_Option : MonoBehaviour
{
    public Button backButton;

    public Button englishButton;
    public Button koreanButton;

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

    public void OnEnglishButton()
    {
        LanguageManager.SetLanguage(LanguageManager.Language.English);
        GameManager.GetInstance().SaveLanguageSetting(LanguageManager.Language.English);
        englishButton.interactable = false;
        koreanButton.interactable = true;
    }

    public void OnKoreanButton()
    {
        LanguageManager.SetLanguage(LanguageManager.Language.Korean);
        GameManager.GetInstance().SaveLanguageSetting(LanguageManager.Language.Korean);
        englishButton.interactable = true;
        koreanButton.interactable = false;
    }
}
