using UnityEngine;
using UnityEngine.UI;

public class UI_TopPanel : MonoBehaviour
{
    public Button homeButton;
    public Button CampaignButton;
    public Button CharacterButton;
    public Button ShopButton;
    public Button exitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        exitButton.onClick.AddListener(OnExit);
    }

    private void OnExit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
