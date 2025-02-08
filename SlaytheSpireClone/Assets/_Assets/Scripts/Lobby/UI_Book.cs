using UnityEngine;
using System.Collections.Generic;
using CCGKit;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.UI;

// 캐릭터 도감
public class UI_Book : MonoBehaviour
{

    [SerializeField] GameObject _EpisodeObject;
    [SerializeField] Button[] _EpisodeButtons;

    Dictionary<string, HeroTemplate> characterList = new Dictionary<string, HeroTemplate>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(var button in _EpisodeButtons)
        {
            button.gameObject.SetActive(false);
        }

        // 캐릭터 파서에서 데이터 불러오기
        for(int i = 0; i < (int)SaveCharacterIndex.Max; i++)
        {
            var handle = Addressables.LoadAssetAsync<HeroTemplate>
                (Parser_CharacterList.GetInstance().GetCharacterAssetReference((SaveCharacterIndex)i));
                

            handle.Completed += heroInfo =>
            {
                var template = heroInfo.Result;
                characterList.Add(template.name, template);
                BindButtonEvent(template);
            };
        }
    }

    // 버튼 이벤트 바인딩
    private void BindButtonEvent(HeroTemplate template)
    {
        // 캐릭터 리스트 순서대로 버튼 이벤트 바인딩. 배열의 이름과 문자열 이름은 반드시 같아야 한다.
        for (int i = 0; i < (int)SaveCharacterIndex.Max; i++)
        {
            if( _EpisodeButtons[i].gameObject.activeSelf == true )
                continue;


            if (characterList.ContainsKey(((SaveCharacterIndex)i).ToString()))
            {
                _EpisodeButtons[i].gameObject.SetActive(true);
                _EpisodeButtons[i].onClick.AddListener(() => OnClickEpisode(template.name));
 
                break;
            }
        }
    }

    public void OnClickEpisode(string characterName)
    {
        var character = characterList[characterName];

        ToolFunctions.FindChild<TextMeshProUGUI>(_EpisodeObject, "EpisodeTitle", true).text = character.CharacterEpisodeTitle;
        ToolFunctions.FindChild<TextMeshProUGUI>(_EpisodeObject, "Episode", true).text = character.CharacterEpisode;

        _EpisodeObject.SetActive(true);
    }
}
