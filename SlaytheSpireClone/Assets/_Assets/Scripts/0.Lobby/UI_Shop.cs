using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CCGKit;
using System.Collections.Generic;

public class UI_Shop : MonoBehaviour
{
    [SerializeField] Button GetGachaButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetGachaButton.onClick.AddListener(OnClickGetGachaButton);
    }

    void OnClickGetGachaButton()
    {
        // TODO : 뽑은 데이터를 표시할 UI 생성


        Debug.Log("GetGachaButton Clicked");

        var characterTemplateList = GameManager.GetInstance().AllcharacterTemplateList;
        var cardTemplateList = GameManager.GetInstance().AllcardTemplateList;

        // 90퍼센트 확률로 카드, 10퍼센트 확률로 캐릭터
        var random = Random.Range(0, 100);
        var randomIndex = 0;
        if (random < 90)
        {
            randomIndex = Random.Range(0, cardTemplateList.Count);

            // 카드 저장
            SaveSystem.GetInstance().SetSaveCardData(cardTemplateList[randomIndex].name);

            Debug.Log(cardTemplateList[randomIndex].name);
            return;
        }
        else
        {
            // 캐릭터 저장
            randomIndex = Random.Range(0, characterTemplateList.Count);
        }

        var randomCharacter = characterTemplateList[randomIndex];

        GameManager.GetInstance().currentCharacter = randomCharacter;

        var handle = Addressables.LoadAssetAsync<HeroTemplate>(randomCharacter);
        handle.Completed += (handle) =>
        {
            var heroTemplate = handle.Result;
            Debug.Log(heroTemplate.name);

            // 같은 캐릭터가 나왔는지 검사
            if (heroTemplate.name.ToString() == SaveSystem.GetInstance().LoadGameData().charGachaData.ToString())
            {
                GachaOverlapCharacterProcess(heroTemplate);
            }
            else
            {
                // 첫 캐릭터
                GachaCharacterProcess(heroTemplate);
            }

        };
    }

    private static void GachaCharacterProcess(HeroTemplate heroTemplate)
    {
        Debug.Log("캐릭터가 나왔습니다.");

        // 캐릭터 저장
        if (heroTemplate.name == "Galahad")
        {
            SaveSystem.GetInstance().SetSaveCharacterData(SaveCharacterIndex.Galahad);
        }
        else if (heroTemplate.name == "Lancelot")
        {
            SaveSystem.GetInstance().SetSaveCharacterData(SaveCharacterIndex.Lancelot);
        }
        else if (heroTemplate.name == "Percival")
        {
            SaveSystem.GetInstance().SetSaveCharacterData(SaveCharacterIndex.Percival);
        }
    }

    private static void GachaOverlapCharacterProcess(HeroTemplate heroTemplate)
    {

        Debug.Log("같은 캐릭터가 나왔습니다.");

        // 중복 캐릭터 저장
        if (heroTemplate.name == "Galahad")
        {
            SaveSystem.GetInstance().SetSaveOverlapCharacterData(new CharacterGachaData(SaveCharacterIndex.Galahad));
        }
        else if (heroTemplate.name == "Lancelot")
        {
            SaveSystem.GetInstance().SetSaveOverlapCharacterData(new CharacterGachaData(SaveCharacterIndex.Lancelot));
        }
        else if (heroTemplate.name == "Percival")
        {
            SaveSystem.GetInstance().SetSaveOverlapCharacterData(new CharacterGachaData(SaveCharacterIndex.Percival));
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
