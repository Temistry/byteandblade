using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenuController : MonoBehaviour
{
    public Button startGameButton; 
    public Button optionsButton;

    [SerializeField] GameObject CharacterSelectPanel; 
    [SerializeField] GameObject OptionPanel;
    void Start()
    {
        // 버튼 클릭 이벤트 등록
        startGameButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(Options);

        CharacterSelectPanel.SetActive(false);
        OptionPanel.SetActive(false);
    }

    private void StartGame() 
    {
        Debug.Log("게임 시작!");
        CharacterSelectPanel.SetActive(true);
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
