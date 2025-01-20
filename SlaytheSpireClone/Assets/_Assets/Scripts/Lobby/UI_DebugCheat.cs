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
    }

    // 치트 적용
    void ApplyCheat(string cheat)
    {
        var command = cheat.Split(' ');
        if (command[0] == "/givegold")
        {
            GameManager.GetInstance().AddGold(int.Parse(command[1]));
            AddChat("[system] Cheat Success");
            return;
        }
        else if (command[0] == "/givemaxhealth")
        {
            GameManager.GetInstance().AddMaxHealth(int.Parse(command[1]));
            AddChat("[system] Cheat Success");
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
