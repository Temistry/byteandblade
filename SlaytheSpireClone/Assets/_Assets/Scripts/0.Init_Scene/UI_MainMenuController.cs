 using UnityEngine;
using UnityEngine.UIElements;

public class UI_MainMenuController : MonoBehaviour
{
    private Button startGameButton;
    private Button cardCollectionButton;
    private Button relicCollectionButton;
    private Button statisticsButton;
    private Button patchNotesButton;
    private Button creditsButton;
    private Button exitButton;
    public GameObject cardSelections; // 카드 선택 UI 
    public GameObject characterSelect; // 캐릭터 선택 UI

    void OnEnable()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); // Full HD 해상도로 설정
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component is missing.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // 버튼 가져오기
        startGameButton = root.Q<Button>("StartGame");
        cardCollectionButton = root.Q<Button>("CardCollection");
        relicCollectionButton = root.Q<Button>("RelicCollection");
        statisticsButton = root.Q<Button>("Statistics");
        patchNotesButton = root.Q<Button>("PatchNotes");
        creditsButton = root.Q<Button>("Credits");
        exitButton = root.Q<Button>("Exit");

        // 버튼 클릭 이벤트 등록
        startGameButton.clicked += StartGame;
        cardCollectionButton.clicked += OpenCardCollections;
        relicCollectionButton.clicked += OpenRelicCollection;
        statisticsButton.clicked += OpenStatistics;
        patchNotesButton.clicked += OpenPatchNotes;
        creditsButton.clicked += OpenCredits;
        exitButton.clicked += QuitGame;
    }

    private void StartGame() 
    {
        Debug.Log("게임 시작!");
        if (characterSelect != null)
        {
            characterSelect.SetActive(true);
        }
    }
    private void OpenCardCollections()
    {
        if (cardSelections != null)
        {
            cardSelections.SetActive(true);
        }
        else
        {
            Debug.LogError("CardSelections 프리팹이 설정되지 않았습니다.");
        }
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
