using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
   private Button startButton;
    private Button optionsButton;
    private Button quitButton;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // 버튼 가져오기
        startButton = root.Q<Button>("Start");
        optionsButton = root.Q<Button>("Option");
        quitButton = root.Q<Button>("Exit");

        // 버튼 클릭 이벤트 등록
        startButton.clicked += StartGame;
        optionsButton.clicked += OpenOptions;
        quitButton.clicked += QuitGame;
    }

    private void StartGame()
    {
        Debug.Log("게임 시작!");
        // 씬 로드 예제
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    private void OpenOptions()
    {
        Debug.Log("옵션 열기");
        // 옵션 메뉴 구현
    }

    private void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }
}
