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
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplate);
        handle.Completed += op =>
        {
            var template = op.Result;
            if (PlayerPrefs.HasKey(saveDataPrefKey))
            {
                var json = PlayerPrefs.GetString(saveDataPrefKey);
                var saveData = JsonUtility.FromJson<SaveData>(json);
                playerDeck.Clear();
                foreach (var id in saveData.Deck)
                {
                    var card = template.StartingDeck.Entries.Find(x => x.Card.Id == id);
                    if (card == null)
                    {
                        card = template.RewardDeck.Entries.Find(x => x.Card.Id == id);
                    }
                    if (card != null)
                    {
                        playerDeck.Add(card.Card);
                    }
                }
            }
            else
            {
                foreach (var entry in template.StartingDeck.Entries)
                {
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
}
