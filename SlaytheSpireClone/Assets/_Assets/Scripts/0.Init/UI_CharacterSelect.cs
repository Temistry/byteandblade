using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UI_CharacterSelect : MonoBehaviour
{

    public Button startButton;
    public Button backButton;

    void Start()
    {
        startButton.onClick.AddListener(OnStart);
        backButton.onClick.AddListener(OnExit);
    }

    private void OnStart()
    {
        Debug.Log("출정 버튼 클릭됨");
        SceneManager.LoadScene("1.Map");
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
