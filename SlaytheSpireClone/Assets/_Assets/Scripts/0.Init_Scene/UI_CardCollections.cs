using UnityEngine;
using UnityEngine.UIElements;

public class UI_CardCollections : MonoBehaviour
{
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var exitButton = root.Q<Button>("Back");

        exitButton.clicked += OnExit;
    }

    private void OnCardSelected(string cardName)
    {
        Debug.Log($"{cardName} 선택됨");
        // 카드 선택 시 로직 추가
    }

    private void OnExit()
    {
        Debug.Log("돌아가기 버튼 클릭됨");
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
