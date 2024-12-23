using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public enum StartRelic
{
    RemoveCard = 0,
    AddGold = 1,
    LoseHealth = 2,
    LoseRelic = 3,
}

public class UI_StartRelic : MonoBehaviour
{
    // 시작 유물 선택지
    Dictionary<int, string> startRelicDescriptions = new Dictionary<int, string>()
    {
        // key : 유물 선택지 인덱스, value : 유물 선택지 설명
        { (int)StartRelic.RemoveCard, "덱에서 카드 1장을 제거합니다." },
        { (int)StartRelic.AddGold, "100 골드를 획득합니다." },
        { (int)StartRelic.LoseHealth, "21 체력 잃고 최대 체력 10 증가" },
        { (int)StartRelic.LoseRelic, "시작 유물을 잃고 무작위 새로운 유물을 획득합니다." },
    };

    public GameObject[] relicButtons;
    public RelicData[] relicData;
    public VerticalLayoutGroup relicLayoutGroup;

    public GameObject popUpUI;

    public GameObject mapObject;
    int selectedRelicIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(GameManager.instance.IsGetStartRelic)
        {
            gameObject.SetActive(false);
            mapObject.SetActive(true);
            return;
        }

        mapObject.SetActive(false);
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
            
            if(i == (int)StartRelic.RemoveCard)
            {
                relicButtons[i].GetComponent<Button>().onClick.AddListener(() => OnRemoveCardClick());
            }
            else if(i == (int)StartRelic.AddGold)
            {
                relicButtons[i].GetComponent<Button>().onClick.AddListener(() => OnAddGoldClick());
            }
            else if(i == (int)StartRelic.LoseHealth)
            {
                relicButtons[i].GetComponent<Button>().onClick.AddListener(() => OnLoseHealthClick());
            }
            else if(i == (int)StartRelic.LoseRelic)
            {
                relicButtons[i].GetComponent<Button>().onClick.AddListener(() => OnResetRelicClick());
            }
            else
            {
                relicButtons[i].GetComponent<Button>().onClick.AddListener(() => 
                {
                    ToolFunctions.FindChild<TMP_Text>(popUpUI, "Discription_Relic", true).text = "그냥 시작합니다.";
                    GameManager.instance.SetIsGetStartRelic(true);
                    popUpUI.SetActive(true);
                });
            }
        }
        
        // 마지막은 유물 얻는 버튼임
        relicButtons[relicButtons.Length - 1].GetComponent<Button>().onClick.AddListener(() => OnRelicButtonClick(randRelic));
    }

    // 덱에서 카드 1장 제거하는 버튼
    void OnRemoveCardClick()
    {
        GameManager.instance.RemoveCard();
         GameManager.instance.SetIsGetStartRelic(true);
        popUpUI.SetActive(true);
    }

    // 100 골드 획득하는 버튼
    void OnAddGoldClick()
    {
        GameManager.instance.AddGold(100);
        GameManager.instance.SetIsGetStartRelic(true);
        popUpUI.SetActive(true);
    }

    // 21 체력 잃고 최대 체력 10 증가하는 버튼
    void OnLoseHealthClick()
    {
        GameManager.instance.LoseHealth(21);
        GameManager.instance.AddMaxHealth(10);
        GameManager.instance.SetIsGetStartRelic(true);
        popUpUI.SetActive(true);
    }

    // 시작 유물 갱신 버튼
    void OnResetRelicClick()
    {
        GameManager.instance.LoseRandomRelic();
        GameManager.instance.AddRandomRelic();
        GameManager.instance.SetIsGetStartRelic(true);
        popUpUI.SetActive(true);
    }

    // 유물 선택 버튼
    void OnRelicButtonClick(int relicIndex)
    {
        Debug.Log("Relic button clicked: " + relicIndex);
        selectedRelicIndex = relicIndex;

        ToolFunctions.FindChild<TMP_Text>(popUpUI, "Discription_Relic", true).text = relicData[relicIndex].relicName;
        ToolFunctions.FindChild<TMP_Text>(popUpUI, "Discription_Relic", true).text += "\n\n" + relicData[relicIndex].description;
        GameManager.instance.SetIsGetStartRelic(true);
        popUpUI.SetActive(true);
    }
}
