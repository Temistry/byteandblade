using UnityEngine;
using CCGKit;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class Parser_CharacterList : Singleton<Parser_CharacterList>
{
    // 캐릭터 이미지 리스트. 순서대로 저장되어 있어야 한다.
    [Tooltip("Character Sprite List. Must Keep Sequence")]
    [SerializeField] private Sprite[] _characterSpriteList;
    public Sprite[] CharacterSpriteList => _characterSpriteList;

    // 모든 캐릭터 템플릿 리스트
    public List<AssetReference> AllcharacterTemplateList;

    public Dictionary<int, CharacterTemplate> characterList = new Dictionary<int, CharacterTemplate>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // AllcharacterTemplateList 초기화
        InitializeCharacterTemplates();
    }

    private void InitializeCharacterTemplates()
    {
        // 캐릭터 템플릿을 초기화하는 로직을 추가합니다.
        // 예시: AllcharacterTemplateList = new List<AssetReference> { /* 캐릭터 템플릿 추가 */ };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AssetReference GetCharacterAssetReference(SaveCharacterIndex index)
    {
        // Parser_CharacterList를 통해 캐릭터 템플릿 리스트에 접근
        return AllcharacterTemplateList[(int)index];
    }
}
