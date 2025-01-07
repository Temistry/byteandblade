using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CCGKit;
using UnityEngine.Diagnostics;
using TMPro;
using Unity.VisualScripting;

public class UI_CardCollections : MonoBehaviour
{
    // 카드 리스트를 저장할 변수
    public List<CardTemplate> cardTemplates; // 카드 템플릿 리스트
    public GameObject cardPrefab; // 카드 프리팹
    public Transform cardContainer; // 카드가 배치될 컨테이너
    public GameObject cardDetailPanel; // 카드 상세 정보를 보여줄 패널
    public Button backButton; // 뒤로가기 버튼
    public CardWidget cardPreviewWidget;


    void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        DisplayCardList();
    }

private int GetSelectedIndex(GameObject item)
    {
        // GridLayoutGroup의 자식 요소들 중에서 선택된 아이템의 인덱스를 찾음
        Transform parent = item.transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).gameObject == item)
            {
                return i; // 인덱스 반환
            }
        }
        return -1; // 아이템이 발견되지 않으면 -1 반환
    }

    private void DisplayCardList()
    {
        var cardList = GameManager.GetInstance().GetCardList();

        cardTemplates.Clear(); // 카드 템플릿 리스트 초기화
        foreach (var card in cardList)
        {
            cardTemplates.Add(card); // 플레이어 덱에 있는 카드를 카드 템플릿 리스트에 추가
        }

        for (int i = 0; i < cardTemplates.Count; i++)
        {
            var card = cardTemplates[i];
            GameObject cardObject = Instantiate(cardPrefab, cardContainer);
            cardObject.GetComponent<CardWidget>().SetInfo(card); // 카드 정보를 설정
            cardObject.GetComponent<Button>().onClick.AddListener(() => OnCardSelected(cardObject)); // 카드 선택 시 이벤트 추가
            cardObject.SetActive(true);
        }
    }

    private void OnCardSelected(GameObject cardObject)
    {
        cardDetailPanel.SetActive(true); // 카드 상세 패널 활성화
        cardPreviewWidget.SetInfo(cardObject.GetComponent<CardWidget>().GetInfo());
    }

    private void OnBackButtonClicked()
    {
        cardDetailPanel.SetActive(false); // 카드 상세 패널 비활성화
    }
}
