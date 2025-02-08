using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CCGKit;

public class UI_CurrentDeck : MonoBehaviour
{
    // 블록 패널
    public GameObject BlockPanel;
    // 카드 UI
    public GameObject CardUI;
    public GameObject CardUI_Selected;
    // 카드 선택 -> 뒤로가기 버튼
    public Button BackButton;

    // 카드 컨텐츠 위치
    public Transform CardContentParent;

    // 카드 컨텐츠 리스트
    List<CardTemplate> CardContents = new List<CardTemplate>();

    // 선택한 카드 인덱스
    private int SelectedCardIndex = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BackButton.onClick.AddListener(OnBack);
    }

    // Update is called once per frame
    void OnEnable()
    {
        // 블록 패널 표시
        if(BlockPanel != null)
        {
            BlockPanel.SetActive(true);
        }

        // 카드 컨텐츠 초기화
        CardContents.Clear();

        // 카드 UI 삭제
        for (int i = 0; i < CardContentParent.childCount; ++i)
            Destroy(CardContentParent.GetChild(i).gameObject);

        List<CardTemplate> cardListContents = new List<CardTemplate>();

        // 게임매니저로부터 소지한 카드 목록 가져오기
        cardListContents = GameManager.GetInstance().GetCardList();
        if (cardListContents == null || cardListContents.Count == 0)
        {
            // 카드 목록이 없으면 캐릭터 카드목록만 표시
            // 캐릭터의 기본 카드 목록 불러오기
            var heroTemplate = GameManager.GetInstance().GetCurrentCharacterTemplate();
            if (heroTemplate != null || heroTemplate.StartingDeck != null)
            {
                foreach (CardLibraryEntry entry in heroTemplate.StartingDeck.Entries)
                {
                    cardListContents.Add(entry.Card);
                }
            }
            else
            {
                // 기본 캐릭터도 없다면 카드 목록 없음
                return;
            }

        }

        // 카드 컨텐츠 생성
        foreach (CardTemplate card in cardListContents)
        {
            // 리스트 추가
            CardContents.Add(card);

            // 카드 UI 설정
            CardUI.GetComponent<CardWidget>().SetInfo(card);

            // 카드 UI 생성
            var ui = Instantiate(CardUI, CardContentParent);

            // 카드 선택 버튼 추가
            ui.GetComponent<Button>().onClick.AddListener(() => OnCardClick(card));
            ui.SetActive(true);
        }
    }

    private void OnBack()
    {
        // 선택한 카드 UI -> 카드 목록으로 뒤로가기
        CardUI_Selected.SetActive(false);

        // 뒤로가기 버튼 숨기기
        BackButton.gameObject.SetActive(false);

        // 블록 패널 숨기기
        if(BlockPanel != null)
        {
            BlockPanel.SetActive(false);
        }
    }

    private void OnCardClick(CardTemplate card)
    {
        SelectedCardIndex = CardContents.IndexOf(card);

        // 선택한 카드 UI 표시
        CardUI_Selected.GetComponent<CardWidget>().SetInfo(card);
        CardUI_Selected.SetActive(true);

        // 뒤로가기 버튼 표시
        BackButton.gameObject.SetActive(true);
    }
}
