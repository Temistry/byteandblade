using System;
using System.Collections.Generic;
using CCGKit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WanzyeeStudio;
using TMPro;
using UnityEngine.UI;

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

    public event Action OnSelectMainCharacter;  // 게임플레이할 메인 캐릭터 선택

    private int maxHealth = 50;
    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            OnMaxHealthChanged?.Invoke();

            SaveData saveData = SaveSystem.GetInstance().LoadGameData();
            saveData.MaxHp = maxHealth;
            SaveSystem.GetInstance().SaveGameData(saveData);

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
                if (value > maxHealth)
                {
                    health = value;
                    SaveData saveData = SaveSystem.GetInstance().LoadGameData();
                    
                    saveData.CurrHp = health;
                    SaveSystem.GetInstance().SaveGameData(saveData);

                     
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
            // 저장
            SaveData saveData = SaveSystem.GetInstance().LoadGameData();
            saveData.gold = gold;
            SaveSystem.GetInstance().SaveGameData(saveData);
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

    #region InGameAllData_Character_Card_Relic_etc
    // 이 게임에서 사용할 수 있는 모든 캐릭터들. 뽑기 데이터에서 사용한다.
    public List<AssetReference> AllcharacterTemplateList;       // 순서 : Galahad, Lancelot, Percival

    // 이 게임에서 사용할 수 있는 모든 카드들. 뽑기 데이터에서 사용한다.
    public List<CardTemplate> AllcardTemplateList;

    // 이 게임에서 사용할 수 있는 모든 유물들. 뽑기 데이터에서 사용한다.
    //public List<RelicTemplate> AllRelicTemplateList;

    #endregion 

    new void Awake()
    {
        base.Awake();
        UpdateUserData();
    }

    public void ReturnToLobbyFromBattle()
    {
        // 전투에서 로비로 돌아옴, 맵 오브젝트를 보여준다.
        FindFirstObjectByType<UI_MainMenuController>().TransitionToMap();
    }

    // 유저 데이터 갱신
    public void UpdateUserData()
    {
        UpdateUserConfigData();

        // 주어진 캐릭터 템플릿으로부터 주소 가능한 자산을 비동기적으로 로드합니다.
        SaveData mySaveData = SaveSystem.GetInstance().LoadGameData();
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(AllcharacterTemplateList[(int)mySaveData.currentCharacterIndex]);

        // 로드가 완료되면 실행할 작업을 정의합니다.
        handle.Completed += heroInfo =>
        {
            var template = heroInfo.Result;

            // 저장된 데이터가 있는지 확인합니다.
            if (PlayerPrefs.HasKey(saveDataPrefKey))
            {
                // 플레이어 덱을 초기화합니다.
                playerDeck.Clear();

                // 현재 캐릭터 정보 불러오기
                var heroTemplate = GetCurrentCharacterTemplate();

                // 저장된 덱 데이터를 반복합니다.
                foreach (var id in mySaveData.Deck)
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

    // 유저 설정 데이터 갱신
    void UpdateUserConfigData()
    {
        SaveData saveData = SaveSystem.GetInstance().LoadGameData();

        Gold = saveData.gold;
        Health = saveData.CurrHp;
        MaxHealth = saveData.MaxHp;
    }

    public AssetReference GetCurrentCharacterAssetReference()
    {
        SaveData mySaveData = SaveSystem.GetInstance().LoadGameData();

        return AllcharacterTemplateList[(int)mySaveData.currentCharacterIndex];
    }

    public HeroTemplate GetCurrentCharacterTemplate()
    {
        SaveData mySaveData = SaveSystem.GetInstance().LoadGameData();
 
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(AllcharacterTemplateList[(int)mySaveData.currentCharacterIndex]);
        return handle.Result;
    }

    void InitDebugPlayerData()
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
        SaveData saveData = SaveSystem.GetInstance().LoadGameData();
        saveData.IsGetStartRelic = value;
        SaveSystem.GetInstance().SaveGameData(saveData);

        if (value)
        {
            UpdateUserData();
            //InitDebugPlayerData();
        }
    }

    public void ResetPlayerData()
    {
        PlayerPrefs.DeleteAll();
        playerDeck.Clear();
        OnResetPlayerData?.Invoke();

        // 캐릭터 추가
        SaveSystem.GetInstance().SetSaveCharacterData(SaveCharacterIndex.Galahad);
        // 현재 캐릭터 설정
        SaveSystem.GetInstance().SetCurrentCharacterIndex(SaveCharacterIndex.Galahad);
        // 메인 UI에서 현재 캐릭터 보여주기
        FindFirstObjectByType<UI_MainMenuController>().LoadMainCharacterActivate();
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

        SaveData saveData = SaveSystem.GetInstance().LoadGameData();
        // 저장 데이터로 정제
        List<int> saveDeck = new List<int>();
        foreach (var card in playerDeck)
            saveDeck.Add(card.Id);

        saveData.Deck = saveDeck;

        SaveSystem.GetInstance().SaveGameData(saveData);
    }

    public void AddGold(int value)
    {
        Gold += value;
    }

    public bool UseGold(int value)
    {
        if (Gold >= value)
        {
            Gold -= value;
            return true;
        }
        return false;
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

        SaveData saveData = SaveSystem.GetInstance().LoadGameData();
        // 저장 데이터로 정제
        List<int> saveDeck = new List<int>();
        foreach (var carddata in playerDeck)
            saveDeck.Add(carddata.Id);

        saveData.Deck = saveDeck;

        SaveSystem.GetInstance().SaveGameData(saveData);
    }

    public void RemoveCard(CardTemplate card)
    {
        playerDeck.Remove(card);

        SaveData saveData = SaveSystem.GetInstance().LoadGameData();
        // 저장 데이터로 정제
        List<int> saveDeck = new List<int>();
        foreach (var carddata in playerDeck)
            saveDeck.Add(carddata.Id);

        saveData.Deck = saveDeck;

        SaveSystem.GetInstance().SaveGameData(saveData);
    }

    public bool IsContinueGame()
    {
        return SaveSystem.GetInstance().LoadGameData().IsGetStartRelic;
    }
}
