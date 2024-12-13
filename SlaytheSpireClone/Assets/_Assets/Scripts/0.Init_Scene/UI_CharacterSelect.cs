using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class UI_CharacterSelect : MonoBehaviour
{
    void Start()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component is missing!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Root VisualElement is null!");
            return;
        }

        var exitButton = root.Q<Button>("Back");
        if (exitButton == null)
        {
            Debug.LogError("Exit button not found in UXML!");
            return;
        }
        exitButton.clicked += OnExit;

        // 캐릭터 선택 버튼 이벤트 등록
        SetupCharacterButton(root, "Character1", "캐릭터 1");
        SetupCharacterButton(root, "Character2", "캐릭터 2");
        SetupCharacterButton(root, "Character3", "캐릭터 3");
        SetupCharacterButton(root, "Character4", "캐릭터 4");
        SetupCharacterButton(root, "Character5", "캐릭터 5");

        // 출정버튼
        var startButton = root.Q<Button>("Start");
        startButton.clicked += OnStart;
    }

    private void OnStart()
    {
        Debug.Log("출정 버튼 클릭됨");
        SceneManager.LoadScene("1.Map");
    }

    private void SetupCharacterButton(VisualElement root, string buttonName, string characterName)
    {
        var button = root.Q<Button>(buttonName);
        if (button == null)
        {
            Debug.LogError($"{buttonName} button not found in UXML!");
            return;
        }
        button.clicked += () => OnCharacterSelected(characterName);
    }

    private void OnCharacterSelected(string characterName)
    {
        Debug.Log($"{characterName} 버튼 클릭됨");
    }

    private void OnExit()
    {
        Debug.Log("돌아가기 버튼 클릭됨");
        gameObject.SetActive(false);
    }
}
