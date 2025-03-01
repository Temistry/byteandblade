using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using CCGKit;
using System.Collections;

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
    
    
    GameObject CurrentCharacter; // 현재 캐릭터

    string continueGameText = "이어하기";
    bool bLateOnceUpdate = false;

    void Start()
    {
        // DOTween 초기화 및 용량 설정
        DOTween.SetTweensCapacity(1000, 50);

        if (InitVideoLoop != null)
        {
            var videoPlayer = InitVideoLoop.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.skipOnDrop = true;  // 타임스탬프 스큐 무시
            }
        }

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
    public void LoadMainCharacterActivate(SaveCharacterIndex characterIndex)
    {
        // 캐릭터 이름 표시
        ToolFunctions.FindChild<TextMeshProUGUI>(CurrentCharacterUI, "Name", true).text = characterIndex.ToString();

        // 캐릭터 이미지 표시
        ToolFunctions.FindChild<Image>
        (CurrentCharacterUI, "Placeholder Model", true).sprite =
         Parser_CharacterList.GetInstance().CharacterSpriteList[(int)characterIndex];

        Invoke("ActiveCharacterUI", 0.1f);
    }

    void ActiveCharacterUI()
    {
        CurrentCharacterUI.SetActive(true);
    }

    private void OnEnable()
    {
        // GameManager 참조를 코루틴으로 지연 호출
        StartCoroutine(DelayedCharacterLoad());
    }

    private IEnumerator DelayedCharacterLoad()
    {
        // GameManager가 초기화될 시간을 확보
        yield return new WaitForSeconds(0.1f);
        
        // 이제 GameManager 참조
        if (GameManager.GetInstance() != null)
        {
            var characterIndex = GameManager.GetInstance().GetCurrentCharacterIndex();
            if(characterIndex != SaveCharacterIndex.Max)
            {
                LoadMainCharacterActivate(characterIndex);
            }
        }
    }

    public void SetCurrentCharacter(SaveCharacterIndex characterIndex)
    {
        CurrentCharacterUI.SetActive(true);
        LoadMainCharacterActivate(characterIndex);
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

        Sequence sequence = DOTween.Sequence();

        if (InitVideo != null)
        {
            InitVideo.SetActive(true);
            var videoPlayer = InitVideo.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.Play();
            }
        }
        if (InitVideoLoop != null) InitVideoLoop.SetActive(false);

        if (TopPanel != null)
        {
            sequence.Join(TopPanel.transform.DOLocalMoveY(1000, 1f));
        }

        if (homeButtonPanel != null)
        {
            sequence.Join(homeButtonPanel.transform.DOLocalMoveX(2000, 1f));
        }

        sequence.OnComplete(() => {
            if (InitVideo != null)
            {
                var videoPlayer = InitVideo.GetComponent<VideoPlayer>();
                Invoke("TransitionToMap", 1f);
                
            }
        });
    }

    public void TransitionToMap()
    {
        Sequence sequence = DOTween.Sequence();

        if (InitVideo != null && InitVideo.activeInHierarchy)
        {
            var canvasGroup = InitVideo.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                sequence.Append(canvasGroup.DOFade(0, 2.5f));
                sequence.AppendCallback(() => {
                    canvasGroup.DOFade(1, 0.5f);
                    InitVideo.SetActive(false);
                    if (InitVideoLoop != null) InitVideoLoop.SetActive(false);
                });
            }
        }

        if (lobbyMenu != null && lobbyMenu.activeInHierarchy)
        {
            var canvasGroup = lobbyMenu.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                sequence.Append(canvasGroup.DOFade(0, 1.5f));
                sequence.AppendCallback(() => {
                    canvasGroup.blocksRaycasts = false;
                });
            }
        }

        // 모든 애니메이션이 완료된 후 씬 전환
        sequence.OnComplete(() => {
            Transition.LoadLevel("1.Map", 0.5f, Color.black);
        });

        // 시퀀스가 비어있다면 바로 씬 전환
        if (sequence.Duration() <= 0)
        {
            Transition.LoadLevel("1.Map", 0.5f, Color.black);
        }
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

    private void OnDestroy()
    {
        // 모든 DOTween 애니메이션 정리
        DOTween.Kill(InitVideo);
        DOTween.Kill(lobbyMenu);
        DOTween.Kill(TopPanel);
        DOTween.Kill(homeButtonPanel);
    }
}
