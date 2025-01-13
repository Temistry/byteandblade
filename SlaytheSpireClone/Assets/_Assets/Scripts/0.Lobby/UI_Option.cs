using UnityEngine;
using UnityEngine.UI;

public class UI_Option : MonoBehaviour
{
    public Button backButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backButton.onClick.AddListener(OnBack);
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
