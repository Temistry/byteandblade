using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CCGKit;

public class UI_CharacterRewardView : MonoBehaviour
{
    [Header("Right UI")]
    [SerializeField]
    private TextMeshProUGUI _imageCaptionCharacterName;
    [SerializeField]
    private TextMeshProUGUI _imageCaptionCharacterDescription;
    [SerializeField]
    private Image _characterImage;

    [Header("Left UI")]
    [SerializeField]
    private TextMeshProUGUI _characterNameText;
    [SerializeField]
    private TextMeshProUGUI _characterCountText;
    [SerializeField]
    private TextMeshProUGUI _characterDescriptionText;

    [Header("Hero Template")]
    [SerializeField]
    private HeroTemplate[] _heroTemplates;

    public void SetInfo(CharacterPieceData characterPieceData)
    {
        // 배열 내에서 캐릭터 이름 비교해서 찾기
        HeroTemplate herodata = FindHeroTemplate(characterPieceData.characterIndex);

        if (herodata != null)
        {
            UpdateUI(herodata, characterPieceData);
        }
    }

    private HeroTemplate FindHeroTemplate(SaveCharacterIndex characterIndex)
    {
        foreach (var heroTemplate in _heroTemplates)
        {
            if (heroTemplate.Name == characterIndex.ToString())
            {
                return heroTemplate;
            }
        }
        return null; // 찾지 못한 경우 null 반환
    }

    private void UpdateUI(HeroTemplate herodata, CharacterPieceData characterPieceData)
    {
        _imageCaptionCharacterName.text = herodata.Name;
        _imageCaptionCharacterDescription.text = herodata.Description;

        _characterNameText.text = herodata.Name;

        // 캐릭터 조각 수 업데이트
        int characterCount = GameManager.GetInstance()
            .GetCharacterPieceData(characterPieceData.characterIndex).count;

        _characterCountText.text = (characterCount + characterPieceData.count).ToString() + " / 10";
        _characterImage.sprite = Resources.Load<Sprite>($"Character/{herodata.Name}");

        // 캐릭터 스펙 설명 업데이트
        _characterDescriptionText.text = $"체력 : {herodata.MaxHealth}\n" +
                                          $"마나 : {herodata.Mana}";
    }
}
