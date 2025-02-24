using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CCGKit;

public class UI_GachaResult : MonoBehaviour
{
    [SerializeField] private GameObject _resultCard;
    [SerializeField] private GameObject _resultCharacter;
    [SerializeField] private Image _background;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _resultCard.SetActive(false);
        _resultCharacter.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetResultCard()
    {
        _resultCard.SetActive(true);

        // 뽑기 UI에서 뽑은 카드 정보를 가져온다.
        var gachaResult = FindFirstObjectByType<UI_Shop>();
        var card = gachaResult.GachaCard;

        // 카드 UI에 정보 세팅
        _resultCard.GetComponent<CardWidget>().SetInfo(card);


        // 카드 이름
        //ToolFunctions.FindChild<TextMeshProUGUI>(_resultCard, "NameText").text = card.name;
        //
        //// 카드 타입 이름
        //ToolFunctions.FindChild<TextMeshProUGUI>(_resultCard, "TypeText").text = card.Type.ToString();
//
        //// 카드 설명
//
        //// 카드 이미지
        //ToolFunctions.FindChild<Image>(_resultCard, "Picture").sprite = card.Picture;
//
        //// 카드 코스트
        //ToolFunctions.FindChild<TextMeshProUGUI>(_resultCard, "CostText").text = card.Cost.ToString();
    }

    public void SetResultCharacter()
    {
        _resultCharacter.SetActive(true);

        // 뽑기 UI에서 뽑은 캐릭터 정보를 가져온다.
        var gachaResult = FindFirstObjectByType<UI_Shop>();
        var character = gachaResult.GachaCharacter;

        // 캐릭터 이름
        ToolFunctions.FindChild<TextMeshProUGUI>(_resultCharacter, "Name", true).text = character.name;

        // 캐릭터 이미지
        for(int i = 0; i < (int)SaveCharacterIndex.Max; i++)
        {
            if(character.name == Parser_CharacterList.GetInstance().CharacterSpriteList[i].name)
            {
                ToolFunctions.FindChild<Image>(_resultCharacter, "Placeholder Model").sprite = Parser_CharacterList.GetInstance().CharacterSpriteList[i];
                break;
            }
        }
    }

    // 활성화 될 때
    void OnEnable()
    {
        // 서서히 알파값이 0에서 1로 만들어진다.
        if(_background != null)
        {
            _background.DOFade(1, 0.5f);
        }
    }

    // 비활성화 될 때
    void OnDisable()
    {
        _resultCard.SetActive(false);
        _resultCharacter.SetActive(false);

        // 알파값을 0으로 만든다.
        if(_background != null)
        {
            _background.DOFade(0, 0.5f);
        }
    }
}
