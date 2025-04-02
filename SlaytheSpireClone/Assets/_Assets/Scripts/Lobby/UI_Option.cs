using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene("0.Lobby");
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
         SceneManager.LoadScene("0.Lobby");
    }

    public void OnKoreanButton()
    {
        LanguageManager.SetLanguage(LanguageManager.Language.Korean);
        GameManager.GetInstance().SaveLanguageSetting(LanguageManager.Language.Korean);
        englishButton.interactable = true;
        koreanButton.interactable = false;
         SceneManager.LoadScene("0.Lobby");
    }
}
