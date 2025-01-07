using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI_MainMenuController : MonoBehaviour
{
    public Button startGameButton; 
    public Button optionsButton;

    string continueGameText = "이어하기";
    bool bLateOnceUpdate = false;
    [SerializeField] GameObject CharacterSelectPanel; 
    [SerializeField] GameObject OptionPanel;

    [SerializeField] GameObject InitVideo; // 게임 시작 누르면 비디오 연출
    [SerializeField] GameObject InitVideoLoop; // 루프 비디오
    void Start()
    {
        // 버튼 클릭 이벤트 등록
        startGameButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(Options);

        CharacterSelectPanel.SetActive(false);
        OptionPanel.SetActive(false);

        GameManager.GetInstance().OnResetPlayerData += ResetPlayerData;
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
                continueGameText = "이어하기";
            }
            else
            {
                continueGameText = "새 게임";
            }
            ToolFunctions.FindChild<TextMeshProUGUI>(startGameButton.gameObject, "Text (TMP)", true).text = continueGameText;
        
            bLateOnceUpdate = true;
        }
       
    }

    private void StartGame() 
    {
        Debug.Log("게임 시작!");

        InitVideo.SetActive(true);
        InitVideoLoop.SetActive(false);

        // 비디오 재생이 끝나면 캐릭터 선택 패널 열기
        InitVideo.GetComponent<VideoPlayer>().Play();

        // 비디오 재생이 끝났는지 확인
        if (!InitVideo.GetComponent<VideoPlayer>().isPlaying)
        {
           Invoke("VideoEnd", 3f);
        }
    }

    void VideoEnd()
    {
        SceneManager.LoadScene("1.Map");
    }

    private void OpenCardCollections()
    {

    }
    private void Options() 
    {
        OptionPanel.SetActive(true);
        Debug.Log("옵션 열기"); 
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
