using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CCGKit;
using System.Collections.Generic;

public class UI_Shop : MonoBehaviour
{
    [SerializeField] Button GetGachaButton;
    [SerializeField] UI_GachaResult _gachaResultUI;
    [SerializeField] CanvasGroup _commonPanel;

    // 뽑은 카드
    CardTemplate _gachaCard;
    public CardTemplate GachaCard
    {
        get {return _gachaCard;}
        private set{_gachaCard = value;}
    }

    // 뽑은 캐릭터
    HeroTemplate _gachaCharacter;
    public HeroTemplate GachaCharacter
    {
        get {return _gachaCharacter;}
        private set{_gachaCharacter = value;}
    }

    void OnEnable()
    {
        _commonPanel.GetComponent<CanvasGroup>().alpha = 1;
    }

    void OnDisable()
    {
        _commonPanel.GetComponent<CanvasGroup>().alpha = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetGachaButton.onClick.AddListener(OnClickGetGachaButton);
    }

    void Reset()
    {
        // 뽑은 가챠 데이터들 초기화
        GachaCard = null;
    }

    void OnClickGetGachaButton()
    {
        // 돈 차감
        if(!GameManager.GetInstance().UseGold(100))
        {
            Debug.Log("돈이 부족합니다.");
            return;
        }

        Reset();

        // UI_GachaResult 활성화
        _gachaResultUI.gameObject.SetActive(true);

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
            SaveSystem.GetInstance().SetSaveCardData(cardTemplateList[randomIndex].Id);

            // 매니저에 카드 추가
            GameManager.GetInstance().AddCard(cardTemplateList[randomIndex]);

            Debug.Log(cardTemplateList[randomIndex].name);

            GachaCard = cardTemplateList[randomIndex];
            
            Invoke("GetCardInvoked", 0.1f);
            return;
        }
        else
        {
            // 캐릭터 저장
            randomIndex = Random.Range(0, characterTemplateList.Count);
        }

        var randomCharacter = characterTemplateList[randomIndex];

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

            GachaCharacter = heroTemplate;
            Invoke("GetCharacterInvoked", 0.1f);
        };
    }

    void GetCardInvoked()
    {
        _gachaResultUI.SetResultCard();
    }

    void GetCharacterInvoked()
    {
        _gachaResultUI.SetResultCharacter();
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
