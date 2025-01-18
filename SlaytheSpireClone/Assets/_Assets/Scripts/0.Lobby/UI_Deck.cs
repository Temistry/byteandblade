using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CCGKit;
using UnityEngine.AddressableAssets;
public class UI_Deck : MonoBehaviour
{
    // 삭제버튼
    public Button DeleteButton;

    // 카드 UI
    public GameObject CardUI;

    // 카드 컨텐츠 위치
    public Transform CardContentParent;

    // 카드 컨텐츠 리스트
    List<CardTemplate> CardContents = new List<CardTemplate>();

    // 선택한 카드 인덱스
    private int SelectedCardIndex = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DeleteButton.onClick.AddListener(OnDelete);
    }

    private void OnDelete()
    {
        // 카드 삭제
        if(SelectedCardIndex == -1)
        {
            return;
        }

        // 매니저에서 삭제
        GameManager.GetInstance().RemoveCard(CardContents[SelectedCardIndex]);

        // 카드 삭제
        CardContents.RemoveAt(SelectedCardIndex);

        // 카드 UI 삭제
        Destroy(CardContentParent.GetChild(SelectedCardIndex).gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        CardContents.Clear();

        // 게임매니저로부터 소지한 카드 목록 가져오기
        List<CardTemplate> cardList = GameManager.GetInstance().GetCardList();

        // 현재 캐릭터 정보 불러오기
        var heroTemplate = GameManager.GetInstance().GetCurrentCharacterTemplate();

        // 캐릭터의 기본 카드 목록 불러오기
        List<CardTemplate> charCardList = new List<CardTemplate>();
        if(heroTemplate != null || heroTemplate.StartingDeck != null)
        {
            foreach(CardLibraryEntry entry in heroTemplate.StartingDeck.Entries)
            {
                charCardList.Add(entry.Card);
            }
        }

        // 캐릭터 카드목록 + 소지한 카드목록. 캐릭터 카드목록이 무조건 위에 있어야함
         List<CardTemplate> MergeCardList = new List<CardTemplate>();
         MergeCardList.AddRange(charCardList);
         MergeCardList.AddRange(cardList);

        // 카드 컨텐츠 생성
        foreach (CardTemplate card in MergeCardList)
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

    private void OnCardClick(CardTemplate card)
    {
        SelectedCardIndex = CardContents.IndexOf(card);
    }
}

