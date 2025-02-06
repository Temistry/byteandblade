using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CCGKit;
using UnityEngine.AddressableAssets;
// 캐릭터 도감
public class UI_Book : MonoBehaviour
{

    //[SerializeField] 

    Dictionary<int, CharacterTemplate> characterList = new Dictionary<int, CharacterTemplate>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 캐릭터 파서에서 데이터 불러오기
        for(int i = 0; i < (int)SaveCharacterIndex.Max; i++)
        {
            var handle = Addressables.LoadAssetAsync<HeroTemplate>
                (Parser_CharacterList.GetInstance().GetCharacterAssetReference((SaveCharacterIndex)i));
                
            var character = handle.Result;

            characterList.Add(i, character);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
