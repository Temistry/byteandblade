using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_DebugCheat : MonoBehaviour
{
    [SerializeField] Transform content;
    [SerializeField] TMP_InputField InputField;
    [SerializeField] GameObject CheatNode;

    List<string> cheatList = new List<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 에디터에서만 이 클래스는 동작한다.
#if UNITY_EDITOR
        SetCheatList();
        InputField.onEndEdit.AddListener(OnValueChanged);
#endif
    }


    void OnValueChanged(string value)
    {
        if (value.Length > 0)
        {
            Debug.Log(value);
            AddChat("[system] Cheat : " + value);

            ApplyCheat(value);
        }
    }

    // 치트 목록 설정
    void SetCheatList()
    {
        cheatList.Clear();
        cheatList.Add("/giveGold");
        cheatList.Add("/giveMaxHealth");
        cheatList.Add("/resetmail");
        cheatList.Add("/resetcharacter");
        cheatList.Add("/addcharacterpiece");
    }

    // 치트 적용
    void ApplyCheat(string cheat)
    {
        var command = cheat.Split(' ');
        // 돈 추가 /givegold 개수
        if (command[0] == "/givegold")
        {
            GameManager.GetInstance().AddGold(int.Parse(command[1]));
            AddChat("[system] Cheat Success");
            return;
        }
        // 체력 /givemaxhealth 개수
        else if (command[0] == "/givemaxhealth")
        {
            GameManager.GetInstance().AddMaxHealth(int.Parse(command[1]));
            AddChat("[system] Cheat Success");
            return;
        }
        // 메일 초기화 /resetmail
        else if (command[0] == "/resetmail")
        {
            SaveSystem.GetInstance().ResetSaveMailData();
            Parser_MailList.GetInstance().ResetMailList();
            AddChat("[system] Cheat Success");
            return;
        }
        // 캐릭터 초기화 /resetcharacter
        else if (command[0] == "/resetcharacter")
        {
            SaveSystem.GetInstance().ResetSaveCharacterData();
            AddChat("[system] Cheat Success");
            return;
        }
        // 특정 캐릭터조각 추가 /addcharacterpiece 캐릭터이름 개수
        else if (command[0] == "/addcharacterpiece")
        {
            for (int i = 0; i < (int)SaveCharacterIndex.Max; i++)
            {
                if (command[1] == ((SaveCharacterIndex)i).ToString())
                {
                    GameManager.GetInstance().AddCharacterPieceData((SaveCharacterIndex)i, int.Parse(command[2]));
                    AddChat($"[system] Cheat Success : {(SaveCharacterIndex)i}");
                    return;
                }
            }

            AddChat("[system] Cheat Code Error");
            return;
        }

        AddChat("[system] Cheat Code Error");
    }

    // 채팅 추가
    private void AddChat(string text)
    {
        var node = Instantiate(CheatNode, content);
        node.GetComponent<TextMeshProUGUI>().text = text;
    }


    void OnEnable()
    {
        // 에디터에서만 이 클래스는 동작한다.
#if !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
