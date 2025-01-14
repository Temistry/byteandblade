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
        // TODO : 뽑기 버튼 클릭 시 게임매니저에 뽑은 후 데이터 저장, PlayerPrefs에 저장
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
            Debug.Log(cardTemplateList[randomIndex].name);

            return;
        }
        else
        {
            randomIndex = Random.Range(0, characterTemplateList.Count);
        }

        var randomCharacter = characterTemplateList[randomIndex];

        GameManager.GetInstance().currentCharacter = randomCharacter;

        var handle = Addressables.LoadAssetAsync<HeroTemplate>(randomCharacter);
        handle.Completed += (handle) =>
        {
            var heroTemplate = handle.Result;
            Debug.Log(heroTemplate.name);

            // 카드와 캐릭터 저장
            List<string> collectedCards = new List<string>(); // 수집한 카드 목록
            // 카드 수집 로직 추가 (예: playerDeck에서 카드 수집)
            
            SaveSystem.GetInstance().SetSaveCollectedData(collectedCards, heroTemplate.name); // 카드와 캐릭터 저장
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
