using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button continueButton;
    private Button quitBattleButton;
    private Button encyclopediaButton;
    private Button statisticsButton;
    private Button settingsButton;
    private Button patchNotesButton;
    private Button quitButton;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // 버튼 가져오기
        continueButton = root.Q<Button>("Continue");
        quitBattleButton = root.Q<Button>("QuitBattle");
        encyclopediaButton = root.Q<Button>("Encyclopedia");
        statisticsButton = root.Q<Button>("Statistics");
        settingsButton = root.Q<Button>("Settings");
        patchNotesButton = root.Q<Button>("PatchNotes");
        quitButton = root.Q<Button>("Exit");

        // 버튼 클릭 이벤트 등록
        continueButton.clicked += ContinueGame;
        quitBattleButton.clicked += QuitBattle;
        encyclopediaButton.clicked += OpenEncyclopedia;
        statisticsButton.clicked += OpenStatistics;
        settingsButton.clicked += OpenSettings;
        patchNotesButton.clicked += OpenPatchNotes;
        quitButton.clicked += QuitGame;
    }

    private void ContinueGame()
    {
        Debug.Log("게임 계속하기!");
        // 씬 로드 예제
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    private void QuitBattle()
    {
        Debug.Log("전투를 포기합니다.");
        // 전투 포기 로직 구현
    }

    private void OpenEncyclopedia()
    {
        Debug.Log("백과사전 열기");
        // 백과사전 메뉴 구현
    }

    private void OpenStatistics()
    {
        Debug.Log("통계 열기");
        // 통계 메뉴 구현
    }

    private void OpenSettings()
    {
        Debug.Log("설정 열기");
        // 설정 메뉴 구현
    }

    private void OpenPatchNotes()
    {
        Debug.Log("패치 노트 열기");
        // 패치 노트 메뉴 구현
    }

    private void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }
}
