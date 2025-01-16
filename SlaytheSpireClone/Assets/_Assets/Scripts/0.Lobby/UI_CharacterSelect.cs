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
        }
    }
    private void OnActivate()
    {


        // 현재 캐릭터가 소지한 캐릭터인가
        var mySaveData = SaveSystem.GetInstance().LoadGameData();
        if (mySaveData.SaveCharacterIndexList.Contains((SaveCharacterIndex)snapScroll.GetIndex()))
        {
            // 다른 캐릭터들 모두 선택 해제
            for (int i = 0; i < (int)SaveCharacterIndex.Max; i++)
            {
                ToolFunctions.FindChild<Image>(CharacterList[i], "ActiveCheck", true).gameObject.SetActive(false);
            }
            SaveSystem.GetInstance().SetCurrentCharacterIndex(SaveCharacterIndex.None);

            // 현재 캐릭터 선택
            isActive = ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text == "Activate";

            if (isActive)
            {
                ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text = "Disable";
                ToolFunctions.FindChild<Image>(CharacterList[snapScroll.GetIndex()], "ActiveCheck", true).gameObject.SetActive(true);

                SaveSystem.GetInstance().SetCurrentCharacterIndex((SaveCharacterIndex)snapScroll.GetIndex());

                FindFirstObjectByType<UI_MainMenuController>().SetCurrentCharacter((SaveCharacterIndex)snapScroll.GetIndex());
            }
            else
            {
                ToolFunctions.FindChild<TextMeshProUGUI>(ActiveButton.gameObject, "Text", true).text = "Activate";
                ToolFunctions.FindChild<Image>(CharacterList[snapScroll.GetIndex()], "ActiveCheck", true).gameObject.SetActive(false);
                FindFirstObjectByType<UI_MainMenuController>().SetCurrentCharacter(SaveCharacterIndex.None);
            }

            GameManager.GetInstance().UpdateUserData();
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
