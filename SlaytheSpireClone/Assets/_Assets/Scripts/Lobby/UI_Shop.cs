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
    [SerializeField] GameObject TopPanel;

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
        // 탑 패널 비활성화
        TopPanel.gameObject.SetActive(false);
        _commonPanel.GetComponent<CanvasGroup>().alpha = 1;
    }

    void OnDisable()
    {
        // 탑 패널 활성화
        TopPanel.gameObject.SetActive(true);

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
            UI_MessageBox.CreateMessageBox("돈이 부족합니다.", ()=>Debug.Log("OK"), null);
            return;
        }

        Reset();

        // UI_GachaResult 활성화
        _gachaResultUI.gameObject.SetActive(true);

        Debug.Log("GetGachaButton Clicked");

        var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
        var cardTemplateList = GameManager.GetInstance().GetCardList();

        // 90퍼센트 확률로 카드, 10퍼센트 확률로 캐릭터
        var random = Random.Range(0, 100);
        var randomIndex = 0;
        if (random < 90)
        {
            randomIndex = Random.Range(0, cardTemplateList.Count);

            // 카드 추가
            GameManager.GetInstance().AddCard(cardTemplateList[randomIndex]);

            Debug.Log(cardTemplateList[randomIndex].name);

            GachaCard = cardTemplateList[randomIndex];
            
            Invoke("GetCardInvoked", 0.1f);
            return;
        }
        else
        {
            // 캐릭터 뽑기
            randomIndex = Random.Range(0, characterTemplateList.Count);
        }

        var randomCharacter = characterTemplateList[randomIndex];

        var handle = Addressables.LoadAssetAsync<HeroTemplate>(randomCharacter);
        handle.Completed += (handle) =>
        {
            var heroTemplate = handle.Result;
            Debug.Log(heroTemplate.name);

            // 같은 캐릭터가 나왔는지 검사
            if (heroTemplate.name.ToString() == GameManager.GetInstance().GetCharacterGachaData().characterIndex.ToString())
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
        var gameManager = GameManager.GetInstance();

        if (heroTemplate.name == "Galahad")
        {
            gameManager.AddCharacter(SaveCharacterIndex.Galahad);
        }
        else if (heroTemplate.name == "Lancelot")
        {
            gameManager.AddCharacter(SaveCharacterIndex.Lancelot);
        }
        else if (heroTemplate.name == "Percival")
        {
            gameManager.AddCharacter(SaveCharacterIndex.Percival);
        }

        gameManager.Save(); // 변경사항 저장
    }

    private static void GachaOverlapCharacterProcess(HeroTemplate heroTemplate)
    {
        Debug.Log("같은 캐릭터가 나왔습니다.");
        var gameManager = GameManager.GetInstance();

        if (heroTemplate.name == "Galahad")
        {
            gameManager.AddOverlapCharacter(SaveCharacterIndex.Galahad);
        }
        else if (heroTemplate.name == "Lancelot")
        {
            gameManager.AddOverlapCharacter(SaveCharacterIndex.Lancelot);
        }
        else if (heroTemplate.name == "Percival")
        {
            gameManager.AddOverlapCharacter(SaveCharacterIndex.Percival);
        }

        gameManager.Save(); // 변경사항 저장
    }

    // Update is called once per frame
    void Update()
    {

    }
}
