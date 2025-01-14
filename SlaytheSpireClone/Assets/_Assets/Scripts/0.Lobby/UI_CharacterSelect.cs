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
    [SerializeField] GameObject[] CharacterList;     // 전체 캐릭터 프리팹들

    bool isActive = false;

    string currentCharacterName;

    void Start()
    {
        ActiveButton.onClick.AddListener(OnActivate);
        snapScroll = ToolFunctions.FindChild<HorizontalScrollSnap>(gameObject, "CharacterSnap", true);

    }

    void OnEnable()
    {
        isActive = ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text == "Activate";

        // 게임매니저로부터 캐릭터 리스트 받기
        var myCharList = SaveSystem.GetInstance().LoadGameData().SaveCharacterIndexList;

        for (int i = 0; i < (int)SaveCharacterIndex.Max; i++)
        {
            // 소지하지 않은 캐릭터는 이미지를 흑백처리
            if (!myCharList.Contains((SaveCharacterIndex)i))
            {
                ToolFunctions.FindChild<Image>(CharacterList[i], "Placeholder Model", true).color = new Color(0.1f, 0.1f, 0.1f, 1f);
            }

            // TODO : 캐릭터 데이터 UI에 연동시키기
        }
    }
    private void OnActivate()
    {
        isActive = ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text == "Activate";

        if (isActive)
        {
            ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text = "Disable";
            ToolFunctions.FindChild<Image>(CharacterList[snapScroll.GetIndex()], "ActiveCheck", true).gameObject.SetActive(true);

            SaveSystem.GetInstance().SetCurrentCharacterIndex((SaveCharacterIndex)snapScroll.GetIndex());
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
        //if(snapScroll.GetIndex() != -1)
        //{
        //    currentCharacterName = ToolFunctions.FindChild<TextMeshProUGUI>(CharacterList[snapScroll.GetIndex()], "Name", true).text;
        //}
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
