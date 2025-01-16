using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI_MainMenuController : MonoBehaviour
{
    [Header("Main UI Buttons")]
    public Button startGameButton; 
    public Button optionsButton;

    [Header("Top UI Buttons")]
    [SerializeField] GameObject TopPanel;   // 상단 패널
    [SerializeField] Button campaignButton;
    [SerializeField] GameObject CharacterSelectPanel; 
    [SerializeField] GameObject OptionPanel;


    [Header("Video")]
    [SerializeField] GameObject InitVideo; // 게임 시작 누르면 비디오 연출
    [SerializeField] GameObject InitVideoLoop; // 루프 비디오

  

    [Header("Panels")]
    [SerializeField] GameObject homeButtonPanel; // 버튼 패널
    [SerializeField] GameObject campaignPanel; // 캠페인 패널
    [SerializeField] GameObject lobbyMenu; // 로비 메뉴
    [SerializeField] GameObject CurrentCharacterUI; // 현재 캐릭터 UI
    

    [Header("InGame")]
    [SerializeField] GameObject lobbyMap;   // 로비 맵
    
    GameObject CurrentCharacter; // 현재 캐릭터

    string continueGameText = "이어하기";
    bool bLateOnceUpdate = false;

    void Start()
    {
        CurrentCharacterUI.SetActive(false);

        // 버튼 클릭 이벤트 등록
        startGameButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(Options);

        CharacterSelectPanel.SetActive(false);
        OptionPanel.SetActive(false);

        GameManager.GetInstance().OnResetPlayerData += ResetPlayerData;
    
        campaignButton.onClick.AddListener(ToggleCampaignPanel);
    }

    // 선택되었던 메인 캐릭터 UI 활성화
    private void LoadMainCharacterActivate()
    {
        var character = GameManager.GetInstance().GetCurrentCharacter();

        if(character == null)
        {
            CurrentCharacterUI.SetActive(false);
            return;
        }

        // 캐릭터 이름 표시
        ToolFunctions.FindChild<TextMeshProUGUI>(CurrentCharacterUI, "Name", true).text = character.editorAsset.name;

        var mySaveData = SaveSystem.GetInstance().LoadGameData();

        // 캐릭터 이미지 표시
        ToolFunctions.FindChild<Image>(CurrentCharacterUI, "Placeholder Model", true).sprite = Parser_CharacterList.GetInstance().CharacterSpriteList[(int)mySaveData.currentCharacterIndex];
    }

    void OnEnable()
    {
        LoadMainCharacterActivate();
    }

    public void SetCurrentCharacter(SaveCharacterIndex characterIndex)
    {
        var  mySaveData = SaveSystem.GetInstance().LoadGameData();

        if(characterIndex == SaveCharacterIndex.None)
        {
            CurrentCharacterUI.SetActive(false);
            mySaveData.currentCharacterIndex = characterIndex;
            SaveSystem.GetInstance().SaveGameData(mySaveData);
            return;
        }

        CurrentCharacterUI.SetActive(true);
        mySaveData.currentCharacterIndex = characterIndex;
        SaveSystem.GetInstance().SaveGameData(mySaveData);
        LoadMainCharacterActivate();
    }

    private void ResetPlayerData()
    {
        bLateOnceUpdate = false;
        Debug.Log("플레이어 데이터 초기화");
    }

    private void Update()
    {
        if(!bLateOnceUpdate)
        {
            if(GameManager.GetInstance().IsContinueGame())
            {
                continueGameText = "Continue";
            }
            else
            {
                continueGameText = "New Game";
            }
            ToolFunctions.FindChild<TextMeshProUGUI>(startGameButton.gameObject, "Title", true).text = continueGameText;
        
            bLateOnceUpdate = true;
        }
       
    }

    public void StartGame() 
    {
        Debug.Log("게임 시작!");

        InitVideo.SetActive(true);
        InitVideoLoop.SetActive(false);

        // 비디오 재생이 끝나면 캐릭터 선택 패널 열기
        InitVideo.GetComponent<VideoPlayer>().Play();

        // 상단패널 위로 올리기
        TopPanel.transform.DOLocalMoveY(1000, 1f);

        // 버튼 패널 오른쪽으로 옮기기
        homeButtonPanel.transform.DOLocalMoveX(2000, 1f);

        // 비디오 재생이 끝났는지 확인
        if (!InitVideo.GetComponent<VideoPlayer>().isPlaying)
        {
           Invoke("VideoEnd", 1f);
        }
    }

    void VideoEnd()
    {
        InitVideo.GetComponent<CanvasGroup>().DOFade(0, 2.5f).OnComplete(() =>
        {
            InitVideo.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
            lobbyMap.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
            lobbyMap.GetComponent<CanvasGroup>().blocksRaycasts = true;


            InitVideo.SetActive(false);
            InitVideoLoop.SetActive(false);
        });

        lobbyMenu.GetComponent<CanvasGroup>().DOFade(0, 1.5f).OnComplete(() =>
        {
            lobbyMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;
        });

    }

    private void OpenCardCollections()
    {

    }
    private void Options() 
    {
        OptionPanel.SetActive(true);
        Debug.Log("옵션 열기"); 
    }

    void ToggleCampaignPanel()
    {
        if(campaignPanel.activeSelf)
        {
            CloseCampaignPanel();
        }
        else
        {
            OpenCampaignPanel();
        }
    }

    private void OpenCampaignPanel()
    {
        campaignPanel.SetActive(true);
        //campaignPanel.GetComponent<CanvasGroup>().DOFade(1, 0.5f).OnComplete(() =>
        //{
        //    campaignPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //});
    }

    private void CloseCampaignPanel()
    {
        campaignPanel.SetActive(false);
        //campaignPanel.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        //{
        //    campaignPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        //});
    }

    private void OpenRelicCollection() { Debug.Log("유물 모음집 열기"); }
    private void OpenStatistics() { Debug.Log("통계 열기"); }
    private void OpenPatchNotes() { Debug.Log("패치 노트 열기"); }
    private void OpenCredits() { Debug.Log("크레딧 열기"); }
    private void QuitGame() 
    {
        Debug.Log("게임 종료!"); 
        Application.Quit(); 
    }
}
