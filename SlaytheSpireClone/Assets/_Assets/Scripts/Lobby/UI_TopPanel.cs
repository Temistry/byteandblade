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
        Post = 4,
        Deck = 5,
        MaxTopPanel
    }

    public Button homeButton;
    public Button CampaignButton;
    public Button CharacterButton;
    public Button ShopButton;
    public Button PostButton;
    public Button DeckButton;
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
        PostButton.onClick.AddListener(OnPost);
        DeckButton.onClick.AddListener(OnDeck);
    }

    private void OnExit()
    {
        Application.Quit();
    }

    private void OnPost()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(false);
        Panels[(int)TopPanelType.Post].SetActive(true);
        Panels[(int)TopPanelType.Deck].SetActive(false);
    }

    private void OnDeck()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(false);
        Panels[(int)TopPanelType.Post].SetActive(false);
        Panels[(int)TopPanelType.Deck].SetActive(true);
    }

    private void OnHome()
    {
        Panels[(int)TopPanelType.Home].SetActive(true);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(false);
        Panels[(int)TopPanelType.Post].SetActive(false);
        Panels[(int)TopPanelType.Deck].SetActive(false);
    }

    private void OnCampaign()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(true);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(false);
        Panels[(int)TopPanelType.Post].SetActive(false);
        Panels[(int)TopPanelType.Deck].SetActive(false);
    }

    private void OnCharacter()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(true);
        Panels[(int)TopPanelType.Shop].SetActive(false);
        Panels[(int)TopPanelType.Post].SetActive(false);
        Panels[(int)TopPanelType.Deck].SetActive(false);
    }

    private void OnShop()
    {
        Panels[(int)TopPanelType.Home].SetActive(false);
        Panels[(int)TopPanelType.Campaign].SetActive(false);
        Panels[(int)TopPanelType.Character].SetActive(false);
        Panels[(int)TopPanelType.Shop].SetActive(true);
        Panels[(int)TopPanelType.Post].SetActive(false);
        Panels[(int)TopPanelType.Deck].SetActive(false);
    }


}
