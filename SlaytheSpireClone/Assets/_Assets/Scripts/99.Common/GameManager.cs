using System;
using System.Collections.Generic;
using CCGKit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WanzyeeStudio;
using TMPro;

public class GameManager : Singleton<GameManager>
{   
    private readonly string saveDataPrefKey = "save";
    private readonly string playTimePrefKey = "playTime";
    
    #region 플레이어 데이터
    public event Action OnHealthChanged;
    public event Action OnMaxHealthChanged;
    public event Action OnGoldChanged;
    public event Action OnPlayTimeChanged;


    public event Action OnResetPlayerData;

    private int maxHealth = 50;
    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            OnMaxHealthChanged?.Invoke();
        }
    }

    private int health = 50;
    public int Health
    {
        get => health;
        set
        {
            if (health != value)
            {
                if(value > maxHealth)
                {
                    value = maxHealth;
                }

                health = value;
                OnHealthChanged?.Invoke();
            }
        }
    }

    private int gold = 0;
    public int Gold
    {
        get => gold;
        set
        {
            gold = value;
            OnGoldChanged?.Invoke();  
        }
    }

    private float playTime = 0f;
    public float PlayTime
    {
        get => playTime;
        set
        {
            playTime = value;
            OnPlayTimeChanged?.Invoke();
        }
    }

    TimeSpan timeSpan;
    public string PlayTimeString
    {
        get 
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }


    
    #endregion
    
    private List<CardTemplate> playerDeck = new List<CardTemplate>();


    // 이 게임에서 사용할 수 있는 모든 캐릭터들
    public List<AssetReference> characterTemplateList;

    // 현재 유저가 가진 캐릭터 데이터
    List<AssetReference> userCharacterList;
    public List<HeroTemplate> UserCharacterList
    {
        get
        {
            // 주어진 캐릭터 참조 목록을 실제 HeroTemplate 오브젝트로 변환합니다.
            // ConvertAll 메서드를 사용하여 각 캐릭터 참조를 LoadAssetAsync를 통해 비동기적으로 로드하고,
            // WaitForCompletion을 호출하여 로드 완료를 기다립니다.
            return userCharacterList.ConvertAll(x => x.LoadAssetAsync<HeroTemplate>().WaitForCompletion());
        }
    }

    // 현재 선택된 캐릭터
    AssetReference currentCharacter;
    
    public bool IsGetStartRelic 
    { 
        get;
        private set;
    }

    public GameObject CurrentCharacterUI;

    void Awake()
    {
        currentCharacter = characterTemplateList[0];

        // 주어진 캐릭터 템플릿으로부터 주소 가능한 자산을 비동기적으로 로드합니다.
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(currentCharacter);

        // 로드가 완료되면 실행할 작업을 정의합니다.
        handle.Completed += heroInfo =>
        {
            // 로드된 주소 가능한 자산의 결과를 가져옵니다.
            var template = heroInfo.Result;

            // 캐릭터 이름 표시
            ToolFunctions.FindChild<TextMeshProUGUI>(CurrentCharacterUI, "Name", true).text = template.Name;

            // 저장된 데이터가 있는지 확인합니다.
            if (PlayerPrefs.HasKey(saveDataPrefKey))
            {
                // 저장된 데이터를 JSON 문자열로 가져옵니다.
                var json = PlayerPrefs.GetString(saveDataPrefKey);
                // JSON 문자열을 SaveData 객체로 변환합니다.
                var saveData = JsonUtility.FromJson<SaveData>(json);
                // 플레이어 덱을 초기화합니다.
                playerDeck.Clear();
                // 저장된 덱 데이터를 반복합니다.
                foreach (var id in saveData.Deck)
                {
                    // 시작 덱에서 카드를 찾습니다.
                    var card = template.StartingDeck.Entries.Find(x => x.Card.Id == id);
                    // 시작 덱에서 카드를 찾지 못하면 보상 덱에서 찾습니다.
                    if (card == null)
                    {
                        card = template.RewardDeck.Entries.Find(x => x.Card.Id == id);
                    }
                    // 카드를 찾았다면 플레이어 덱에 추가합니다.
                    if (card != null)
                    {
                        playerDeck.Add(card.Card);
                    }
                }
            }
            else
            {
                // 저장된 데이터가 없으면 시작 덱을 플레이어 덱으로 설정합니다.
                foreach (var entry in template.StartingDeck.Entries)
                {
                    // 카드의 복사 수만큼 플레이어 덱에 추가합니다.
                    for (var i = 0; i < entry.NumCopies; i++)
                    {
                        playerDeck.Add(entry.Card);
                    }
                }
            }

        }; 
    }

    void Start()
    {

    }
    
    void InitPlayerData()
    {
        Gold = 0;
        Health = 50;
        MaxHealth = 50;
    }


    public void Update()
    {
        UpdatePlayTime();
    }

    public List<CardTemplate> GetCardList()
    {
        return playerDeck;
    }

    public void SetIsGetStartRelic(bool value)
    {
        IsGetStartRelic = value;
        if(value)
        {
            InitPlayerData();
        }
    }

    public void ResetPlayerData()
    {
        PlayerPrefs.DeleteAll();
        playerDeck.Clear();
        OnResetPlayerData?.Invoke();
    }
    void UpdatePlayTime()
    {
        // 플레이 시간 업데이트
        PlayTime += Time.deltaTime;
        timeSpan = TimeSpan.FromSeconds(playTime);
    }

    public void SavePlayTime()
    {
        PlayerPrefs.SetFloat(playTimePrefKey, playTime);
    }

    public void RemoveCard()
    {
        playerDeck.RemoveAt(UnityEngine.Random.Range(0, playerDeck.Count));
    }

    public void AddGold(int value)
    {
        Gold += value;
    }

    public void LoseHealth(int value)
    {
        Health -= value;
    }

    public void AddMaxHealth(int value)
    {
        MaxHealth += value;
    }

    public void LoseRandomRelic()
    {
        // 유물 리스트 중 랜덤으로 하나 제거
        Debug.Log("LoseRandomRelic");
    }

    public void AddRandomRelic()
    {
        // 유물 리스트 중 랜덤으로 하나 추가
        Debug.Log("AddRandomRelic");
    }

    public void AddCard(CardTemplate card)
    {
        playerDeck.Add(card);
    }

    public void RemoveCard(CardTemplate card)
    {
        playerDeck.Remove(card);
    }

    public bool IsContinueGame()
    {
        return PlayerPrefs.HasKey(saveDataPrefKey);
    }
}
