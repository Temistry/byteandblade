using System;
using System.Collections.Generic;
using CCGKit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WanzyeeStudio;

public class GameManager : BaseSingleton<GameManager>
{   
    private readonly string saveDataPrefKey = "save";
    private readonly string mapPrefKey = "map";
    private readonly string playTimePrefKey = "playTime";
    
    #region 플레이어 데이터
    public event Action OnHealthChanged;
    public event Action OnMaxHealthChanged;
    public event Action OnGoldChanged;
    public event Action OnPlayTimeChanged;

    private int maxHealth;
    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            OnMaxHealthChanged?.Invoke();
        }
    }

    private int health;
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

    private int gold;
    public int Gold
    {
        get => gold;
        set
        {
            if (gold != value)
            {
                gold = value;
                OnGoldChanged?.Invoke();
            }
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
    public AssetReference characterTemplate;
    public bool IsGetStartRelic 
    { 
        get;
        private set;
    }

    protected override void Awake()
    {
        base.Awake();
        // 주어진 캐릭터 템플릿으로부터 주소 가능한 자산을 비동기적으로 로드합니다.
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplate);
        // 로드가 완료되면 실행할 작업을 정의합니다.
        handle.Completed += op =>
        {
            // 로드된 주소 가능한 자산의 결과를 가져옵니다.
            var template = op.Result;
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
        InitPlayerData();
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
    }

    public void ResetPlayerData()
    {
        PlayerPrefs.DeleteAll();
        playerDeck.Clear();
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
