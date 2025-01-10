using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Diagnostics;
using TMPro;

public class UI_CharacterSelect : MonoBehaviour
{

    public Button ActiveButton; // 활성화 버튼
    public GameObject listContent; // 캐릭터 리스트 컨텐츠

    HorizontalScrollSnap snapScroll;    // 캐릭터선택창 스와이프 기능
    GameObject[] CharacterList;

    bool isActive = false;

    void Start()
    {
        ActiveButton.onClick.AddListener(OnActivate);
    
        snapScroll = ToolFunctions.FindChild<HorizontalScrollSnap>(gameObject, "CharacterSnap", true);

        isActive = ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text == "Activate";

        CharacterList = new GameObject[listContent.transform.childCount];

        for(int i = 0; i < listContent.transform.childCount; i++)
        {
            CharacterList[i] = listContent.transform.GetChild(i).gameObject;
        }
    }

    private void OnActivate()
    {
        isActive = ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text == "Activate";

        if(isActive)
        {
            ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text = "Disable";
            ToolFunctions.FindChild<Image>(CharacterList[snapScroll.GetIndex()], "ActiveCheck", true).gameObject.SetActive(true);
        }
        else
        {
            ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text = "Activate";
            ToolFunctions.FindChild<Image>(CharacterList[snapScroll.GetIndex()], "ActiveCheck", true).gameObject.SetActive(false);
        }
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
