using UnityEngine;

public class Parser_CharacterList : Singleton<Parser_CharacterList>
{
    // 캐릭터 이미지 리스트. 순서대로 저장되어 있어야 한다.
    [Tooltip("Character Sprite List. Must Keep Sequence")]
    [SerializeField] private Sprite[] _characterSpriteList;
    public Sprite[] CharacterSpriteList => _characterSpriteList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
