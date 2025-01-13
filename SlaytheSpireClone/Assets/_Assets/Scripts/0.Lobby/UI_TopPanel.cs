using UnityEngine;
using UnityEngine.UI;

public class UI_TopPanel : MonoBehaviour
{
    enum TopPanelType
    {
        Home = 0,
        Campaign = 1,
        Character = 2,
        Shop = 3,
        MaxTopPanel
    }

    public Button homeButton;
    public Button CampaignButton;
    public Button CharacterButton;
    public Button ShopButton;
    public Button exitButton;

    public GameObject[] Panels;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        exitButton.onClick.AddListener(OnExit);
        homeButton.onClick.AddListener(OnHome);
        CampaignButton.onClick.AddListener(OnCampaign);
        CharacterButton.onClick.AddListener(OnCharacter);
        ShopButton.onClick.AddListener(OnShop);
    }

    private void OnExit()
    {
        Application.Quit();
    }

    private void OnHome()
    {
        Panels[(int)TopPanelType.Home].SetActive(true);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(false);
    }

    private void OnCampaign()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(true);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(false);
    }

    private void OnCharacter()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(true);
        Panels[(int)TopPanelType.Shop].SetActive(false);
    }

    private void OnShop()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(true);
    }


}
