using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_StartRelic : MonoBehaviour
{
    // 시작 유물 선택지
    Dictionary<int, string> startRelicDescriptions = new Dictionary<int, string>()
    {
        { 0, "덱에서 카드 1장을 제거합니다." },
        { 1, "100 골드를 획득합니다." },
        { 2, "21 체력 잃고 최대 체력 10 증가" },
        { 3, "시작 유물을 잃고 무작위 새로운 유물을 획득합니다." },
    };

    public GameObject[] relicButtons;
    public RelicData[] relicData;
    public VerticalLayoutGroup relicLayoutGroup;

    public GameObject popUpUI;

    int selectedRelicIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateRandomRelicButtons();
    }

    void GenerateRandomRelicButtons()
    {
        // relicData에서 랜덤으로 선택된 버튼의 정보를 가져옵니다.
        int randRelic = Random.Range(0, relicData.Length);

        for(int i = 0; i < startRelicDescriptions.Count; i++)
        {
            // 버튼 클릭 이벤트 추가
            relicButtons[i].GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = startRelicDescriptions[i].ToString();
            relicButtons[i].GetComponent<Button>().onClick.AddListener(() => 
            {
                ToolFunctions.FindChild<TMP_Text>(popUpUI, "Discription_Relic", true).text = "그냥 시작합니다.";
                popUpUI.SetActive(true);
            });
        }
        
        // 마지막은 유물 얻는 버튼임
        relicButtons[relicButtons.Length - 1].GetComponent<Button>().onClick.AddListener(() => OnRelicButtonClick(randRelic));
    }

    void OnRelicButtonClick(int relicIndex)
    {
        Debug.Log("Relic button clicked: " + relicIndex);
        selectedRelicIndex = relicIndex;

        ToolFunctions.FindChild<TMP_Text>(popUpUI, "Discription_Relic", true).text = relicData[relicIndex].relicName;
        ToolFunctions.FindChild<TMP_Text>(popUpUI, "Discription_Relic", true).text += "\n\n" + relicData[relicIndex].description;

        popUpUI.SetActive(true);
    }
}
