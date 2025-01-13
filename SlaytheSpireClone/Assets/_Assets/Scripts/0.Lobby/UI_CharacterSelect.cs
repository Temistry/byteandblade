using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Diagnostics;
using TMPro;
using UnityEngine.AddressableAssets;

public class UI_CharacterSelect : MonoBehaviour
{

    public Button ActiveButton; // 활성화 버튼
    public GameObject listContent; // 캐릭터 리스트 컨텐츠

    HorizontalScrollSnap snapScroll;    // 캐릭터선택창 스와이프 기능
    GameObject[] CharacterList;

    bool isActive = false;

    public GameObject characterProfilePrefab;

    string currentCharacterName;

    void Start()
    {
        ActiveButton.onClick.AddListener(OnActivate);
    
        snapScroll = ToolFunctions.FindChild<HorizontalScrollSnap>(gameObject, "CharacterSnap", true);

        isActive = ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text == "Activate";

        // 게임매니저로부터 캐릭터 리스트 받기
        var characterList = GameManager.GetInstance().UserCharacterList;

        CharacterList = new GameObject[characterList.Count];

        for(int i = 0; i < characterList.Count; i++)
        {
            CharacterList[i] = Instantiate(characterProfilePrefab, listContent.transform);

            // TODO : 캐릭터 데이터 UI에 연동시키기
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

    void Update()
    {
        // 현재 캐릭터 이름 업데이트
        if(snapScroll.GetIndex() != -1)
        {
            currentCharacterName = ToolFunctions.FindChild<TextMeshProUGUI>(CharacterList[snapScroll.GetIndex()], "Name", true).text;
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
