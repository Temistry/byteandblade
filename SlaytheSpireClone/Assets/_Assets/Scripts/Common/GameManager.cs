using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WanzyeeStudio;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CCGKit;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    private SaveData playerData;
    private Map currentMap;
    private SaveSystem.ISaveSystem saveSystem;

    [SerializeField] private GameObject loadingScreenPrefab;

    public event Action<string> OnRegiserNickName;
    public event Action OnHealthChanged;
    public event Action OnGoldChanged;
    public event Action<string> OnPlayTimeChanged;
    public event Action OnResetPlayerData;
    //public event Action OnSelectMainCharacter;  // 게임플레이할 메인 캐릭터 선택

    // 캐싱된 데이터들은 playerData의 참조를 유지
    private SavePlayerStats playerStats => playerData.stats;
    private SaveCharacterData characterData => playerData.characterData;
    private DeckData deckData => playerData.deckData;
    private MailboxData mailbox => playerData.mailbox;
    private GameProgressData progress => playerData.progress;

    // 캐싱된 데이터들
    private string nickName = "";
    private int maxHealth;
    private int health;
    private int gold;
    private float playTime;
    private TimeSpan timeSpan;
    private float lastUpdateTime = 0f;
    private List<CharacterPieceData> characterPieceDataList;
    private List<CardTemplate> playerDeck = new List<CardTemplate>();
    private List<MailData> mailDataList = new List<MailData>();

    // 캐릭터 템플릿 캐싱을 위한 딕셔너리 추가
    private Dictionary<SaveCharacterIndex, HeroTemplate> characterTemplateCache = new Dictionary<SaveCharacterIndex, HeroTemplate>();

    new void Awake()
    {
        base.Awake();
        saveSystem = SaveSystem.GetInstance().GetManagerInterface();
        playerData = new SaveData();
        
        // 캐릭터 템플릿 미리 로드
        StartCoroutine(PreloadCharacterTemplates());
        
        // 세이브 파일 존재 여부 확인 후 로드
        if (saveSystem.IsSaveDataExist())
        {
            // 세이브 파일이 존재하면 로드
            Load();
        }
        else
        {
            // 세이브 파일이 없으면 새 게임 데이터 초기화
            ResetPlayerData();
        }
    }

    private void InitializeGameData()
    {
        // 이 메서드는 이제 Load 메서드로 대체됨
        Debug.Log("InitializeGameData는 더 이상 사용되지 않습니다. Load 메서드를 사용하세요.");
    }

    public void UpdateUserData()
    {
        try
        {
            // 현재 캐릭터 템플릿 로드
            var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
            var handle = Addressables.LoadAssetAsync<HeroTemplate>(
                characterTemplateList[(int)characterData.currentCharacterIndex]);

            handle.Completed += heroInfo =>
            {
                try
                {
                    var template = heroInfo.Result;
                    
                    // 플레이어 덱 초기화
                    playerDeck.Clear();

                    // 저장된 덱 데이터가 있으면 로드
                    if (deckData != null && deckData.Deck != null && deckData.Deck.Count > 0)
                    {
                        foreach (var id in deckData.Deck)
                        {
                            // 파서에서 카드 템플릿 조회
                            var cardTemplate = Parser_CardList.GetInstance().GetCardTemplate(id);

                            if (cardTemplate != null)
                            {
                                playerDeck.Add(cardTemplate);
                            }
                        }
                    }
                    else
                    {
                        // 저장된 덱이 없으면 기본 덱 로드
                        foreach (var entry in template.StartingDeck.Entries)
                        {
                            for (var i = 0; i < entry.NumCopies; i++)
                            {
                                playerDeck.Add(entry.Card);
                            }
                        }
                        
                        // 기본 덱을 저장 데이터에 반영
                        deckData.Deck.Clear();
                        foreach (var card in playerDeck)
                        {
                            deckData.Deck.Add(card.Id);
                        }
                    }
                    
                    // 저장
                    Save();
                    
                    Addressables.Release(handle);
                }
                catch (Exception e)
                {
                    Debug.LogError($"캐릭터 템플릿 처리 중 오류: {e.Message}");
                    Addressables.Release(handle);
                }
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateUserData 실행 중 오류: {e.Message}");
        }
    }

    void UpdateUserConfigData()
    {
        Gold = gold;
        Health = health;
        MaxHealth = maxHealth;
    }

    public void Update()
    {
        playTime += Time.deltaTime;
        if (Time.time - lastUpdateTime >= 1f)
        {
            lastUpdateTime = Time.time;
            timeSpan = TimeSpan.FromSeconds(playTime);

            // 시, 분, 초
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;

            // 시, 분, 초 형식으로 변환
            string timeString = $"{hours:D2}:{minutes:D2}:{seconds:D2}";

            OnPlayTimeChanged?.Invoke(timeString);
        }
    }

    public void ExitGame()
    {
        Save();
        UI_MessageBox.CreateMessageBox("게임을 종료하시겠습니까?", () =>
        {
            Debug.Log("게임 종료");
            Application.Quit();
        }, () =>
        {
            Debug.Log("게임 종료 취소");
        });
    }

    public void SavePlayTime()
    {
        try
        {
            saveSystem.SavePlayTime(playTime, timeSpan.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"플레이 타임 저장 호출 중 오류: {e.Message}");
        }
    }

    public bool UseGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }
    }

    // 모든 캐릭터 템플릿 미리 로드하는 코루틴
    private IEnumerator PreloadCharacterTemplates()
    {
        var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
        
        for (int i = 0; i < (int)SaveCharacterIndex.Max; i++)
        {
            var index = (SaveCharacterIndex)i;
            var handle = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[i]);
            
            // 비동기 로드 완료 대기
            yield return handle;
            
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                characterTemplateCache[index] = handle.Result;
                Debug.Log($"캐릭터 템플릿 캐싱 완료: {index}");
            }
            else
            {
                Debug.LogError($"캐릭터 템플릿 로드 실패: {index}");
            }
        }
        
        Debug.Log("모든 캐릭터 템플릿 캐싱 완료");
    }

    // 캐싱된 템플릿 반환하는 메서드로 수정
    public HeroTemplate GetCurrentCharacterTemplate()
    {
        var currentIndex = characterData.currentCharacterIndex;
        
        // 캐시에 있으면 바로 반환
        if (characterTemplateCache.TryGetValue(currentIndex, out var template))
        {
            return template;
        }
        
        // 캐시에 없으면 동기적으로 로드 (비상용)
        Debug.LogWarning($"캐릭터 템플릿 캐시 미스: {currentIndex}, 동기 로드 시도");
        var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
        var handle = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[(int)currentIndex]);
        handle.WaitForCompletion();
        
        // 캐시에 저장
        characterTemplateCache[currentIndex] = handle.Result;
        
        return handle.Result;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        OnHealthChanged?.Invoke();
    }

    // 게임을 이어서 할 수 있는지 여부를 반환하는 메서드 추가
    public bool IsContinueGame()
    {
        return saveSystem.IsSaveDataExist();
    }

    // 로딩 화면 인스턴스화
    private GameObject InstantiateLoadingScreen()
    {
        if (loadingScreenPrefab != null)
        {
            return Instantiate(loadingScreenPrefab);
        }
        return null;
    }

    public void Save()
    {
        var loadingScreen = FindFirstObjectByType<UI_LoadingScreen>();
        if (loadingScreen != null)
        {
            Destroy(loadingScreen.gameObject);
        }

        GameObject tempLoadingScreen = InstantiateLoadingScreen();
        StartCoroutine(SaveCoroutine(tempLoadingScreen));
    }

    private IEnumerator SaveCoroutine(GameObject loadingScreen)
    {
        // 저장 시간 시뮬레이션
        float timer = 0;
        float duration = 0.5f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        try
        {
            // 캐싱된 데이터를 SaveData에 반영
            playerStats.NickName = nickName;
            playerStats.MaxHp = maxHealth;
            playerStats.CurrHp = health;
            playerStats.gold = gold;
            characterData.characterPieceDataList = characterPieceDataList;
            mailbox.mailDataList = mailDataList;
            
            // 카드 데이터 저장 - 덱 정보 업데이트
            deckData.Deck.Clear();
            foreach (var card in playerDeck)
            {
                if (card != null)
                {
                    deckData.Deck.Add(card.Id);
                }
            }

            // SaveSystem을 통해 저장
            saveSystem.Save(playerData, currentMap);
            SavePlayTime();
            
            Debug.Log("게임 데이터 저장 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 중 오류 발생: {e.Message}");
        }
        finally
        {
            if (loadingScreen != null && this != null)
            {
                Destroy(loadingScreen);
            }
        }
    }

    public void ResetPlayerData()
    {
        PlayerPrefs.DeleteAll();
        
        // 데이터 초기화
        nickName = "";
        maxHealth = 80;  // 기본 체력
        health = 80;     // 기본 체력
        gold = 0;
        playTime = 0f;
        timeSpan = TimeSpan.Zero;
        lastUpdateTime = 0f;
        
        // 컬렉션 초기화
        characterPieceDataList = new List<CharacterPieceData>();
        playerDeck.Clear();
        mailDataList.Clear();
        
        // SaveData 초기화 (새 인스턴스 생성)
        playerData = new SaveData();
        
        try
        {
            // 기본 캐릭터 설정 (null 체크 추가)
            if (characterData != null && characterData.SaveCharacterIndexList != null)
            {
                characterData.SaveCharacterIndexList.Clear();
                characterData.SaveCharacterIndexList.Add(SaveCharacterIndex.Galahad);
                characterData.currentCharacterIndex = SaveCharacterIndex.Galahad;
            }
            
            // 기본 덱 설정 (예외 처리 추가)
            try
            {
                var characterTemplateList = Parser_CharacterList.GetInstance()?.AllcharacterTemplateList;
                if (characterTemplateList != null && characterTemplateList.Count > 0 && deckData != null)
                {
                    var handle = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[(int)SaveCharacterIndex.Galahad]);
                    handle.WaitForCompletion();
                    
                    if (handle.Result != null)
                    {
                        var template = handle.Result;
                        deckData.Deck.Clear();
                        
                        foreach (var entry in template.StartingDeck.Entries)
                        {
                            deckData.Deck.Add(entry.Card.Id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"기본 덱 설정 중 오류: {e.Message}");
            }
            
            // 저장
            Save();
            
            // UI 업데이트
            OnResetPlayerData?.Invoke();
            
            var mainMenu = FindFirstObjectByType<UI_MainMenuController>();
            if (mainMenu != null)
            {
                mainMenu.LoadMainCharacterActivate(SaveCharacterIndex.Galahad);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 초기화 중 오류: {e.Message}");
        }
    }

    public List<CardTemplate> GetCardList() => playerDeck;

    // 씬 전환 시 자동 저장 및 로딩 화면 정리
    private void OnSceneUnloaded(Scene scene)
    {
        Save();
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // 저장된 캐릭터 목록 반환
    public List<SaveCharacterIndex> GetSavedCharacterList()
    {
        return characterData.SaveCharacterIndexList;
    }

    // 현재 캐릭터 인덱스 설정
    public void SetCurrentCharacter(SaveCharacterIndex characterIndex)
    {
        try
        {
            characterData.currentCharacterIndex = characterIndex;

            // 기본 체력값 설정
            int defaultMaxHealth = 80;
            int defaultHealth = 80;

            try
            {
                var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
                if (characterTemplateList != null && characterTemplateList.Count > (int)characterIndex)
                {
                    var template = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[(int)characterIndex]).Result;
                    if (template != null)
                    {
                        defaultMaxHealth = template.MaxHealth;
                        defaultHealth = template.Health;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"캐릭터 템플릿 로드 실패, 기본값 사용: {e.Message}");
            }

            playerStats.MaxHp = defaultMaxHealth;
            playerStats.CurrHp = defaultHealth;

            Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"캐릭터 설정 실패: {e.Message}");
        }
    }

    // 현재 캐릭터 인덱스 반환
    public SaveCharacterIndex GetCurrentCharacterIndex()
    {
        return characterData.currentCharacterIndex;
    }

    // 캐릭터 추가
    public void AddCharacter(SaveCharacterIndex characterIndex)
    {
        if (!characterData.SaveCharacterIndexList.Contains(characterIndex))
        {
            characterData.SaveCharacterIndexList.Add(characterIndex);

            // 캐릭터의 기본 덱 추가
            var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
            var template = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[(int)characterIndex]).Result;
            foreach (var entry in template.StartingDeck.Entries)
            {
                deckData.Deck.Add(entry.Card.Id);
            }
        }
    }

    // 속성들 수정 - SaveSystem 직접 호출 제거
    public string NickName
    {
        get => nickName;
        set
        {
            nickName = value;
            playerStats.NickName = nickName;
            OnRegiserNickName?.Invoke(nickName);
        }
    }

    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            playerStats.MaxHp = maxHealth;
        }
    }

    public int Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            playerStats.CurrHp = health;
            OnHealthChanged?.Invoke();
        }
    }

    public int Gold
    {
        get => gold;
        set
        {
            gold = value;
            playerStats.gold = gold;
            OnGoldChanged?.Invoke();
        }
    }

    public CharacterPieceData GetCharacterPieceData(SaveCharacterIndex characterIndex)
    {
        // 리스트가 null이면 초기화
        if (characterPieceDataList == null)
        {
            characterPieceDataList = new List<CharacterPieceData>();
        }

        // 해당 캐릭터의 조각 데이터를 찾음
        var pieceData = characterPieceDataList.Find(x => x.characterIndex == characterIndex);
        
        // 없으면 새로 생성
        if (pieceData == null)
        {
            pieceData = new CharacterPieceData(characterIndex, 0);
            characterPieceDataList.Add(pieceData);
        }

        return pieceData;
    }

    public void AddCharacterPieceData(SaveCharacterIndex characterIndex, int count)
    {
        var pieceData = GetCharacterPieceData(characterIndex);
        pieceData.count += count;
    }

    // 메일 관련 메서드들 추가
    public List<MailData> GetMailDataList()
    {
        return mailDataList;
    }

    public void AddMailData(MailData mailData)
    {
        mailDataList.Add(mailData);
    }

    public void SetMailRead(string title)
    {
        var mail = mailDataList.Find(x => x.title == title);
        if (mail != null)
        {
            mail.isRead = true;
        }
    }

    // 캐릭터 가챠 데이터 관련
    public CharacterGachaData GetCharacterGachaData()
    {
        return characterData.charGachaData;
    }

    // 카드 추가
    public void AddCard(CardTemplate card)
    {
        playerDeck.Add(card);
        deckData.Deck.Add(card.Id);
    }

    // 캐릭터 중복 처리
    public void AddOverlapCharacter(SaveCharacterIndex characterIndex)
    {
        characterData.charGachaData.characterIndex = characterIndex;
        characterData.charGachaData.overlapCount++;
        if (!characterData.SaveCharacterIndexList.Contains(characterIndex))
        {
            characterData.SaveCharacterIndexList.Add(characterIndex);
        }
    }

    // 맵 관련 메서드들
    public Map GetCurrentMap()
    {
        return currentMap;
    }

    public void SetCurrentMap(Map map)
    {
        currentMap = map;
    }

    public void SaveCurrentMap()
    {
        if (currentMap != null)
        {
            saveSystem.SaveMap(currentMap);
        }
    }

    // 맵 생성 또는 로드
    public Map LoadOrGenerateMap(System.Random rng, MapGenerator mapGenerator)
    {
        if (currentMap == null || !currentMap.HasBossNode())
        {
            currentMap = saveSystem.LoadMap();
            
            // 맵이 없거나 보스 노드가 없는 경우 새 맵 생성
            if (currentMap == null || !currentMap.HasBossNode())
            {
                currentMap = mapGenerator.GenerateMap(rng);
            }
        }
        return currentMap;
    }

    public void ResetMapData()
    {
        // Map 데이터를 초기화하는 로직 추가
        currentMap = null;
        saveSystem.SaveMap(null);
        Debug.Log("Map 데이터가 초기화되었습니다.");
    }

    // 캐릭터 저장 관련 메서드들
    public void SetCharacterData(SaveCharacterIndex characterIndex)
    {
        if (characterData.SaveCharacterIndexList == null)
        {
            characterData.SaveCharacterIndexList = new List<SaveCharacterIndex>();
        }

        // 기본 캐릭터 추가
        if (!characterData.SaveCharacterIndexList.Contains(SaveCharacterIndex.Galahad))
        {
            characterData.SaveCharacterIndexList.Add(SaveCharacterIndex.Galahad);
        }

        // 새 캐릭터 추가
        if (characterIndex != SaveCharacterIndex.Max && 
            !characterData.SaveCharacterIndexList.Contains(characterIndex))
        {
            characterData.SaveCharacterIndexList.Add(characterIndex);
        }
    }

    // 방어도 값 반환
    public int GetShieldValue()
    {
        return playerStats.Shield;
    }

    private void OnApplicationQuit()
    {
        // 로딩 화면 정리
        var loadingScreens = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(go => go.name.Contains("UI_LoadingScreen"));
        foreach (var screen in loadingScreens)
        {
            Destroy(screen);
        }
    }

    // 세이브 파일 로드 함수
    public void Load()
    {
        try
        {
            GameObject tempLoadingScreen = InstantiateLoadingScreen();
            StartCoroutine(LoadCoroutine(tempLoadingScreen));
        }
        catch (Exception e)
        {
            Debug.LogError($"로드 시작 중 오류: {e.Message}");
        }
    }

    // 로드 코루틴
    private IEnumerator LoadCoroutine(GameObject loadingScreen)
    {
        // 로딩 시간 시뮬레이션
        float timer = 0;
        float duration = 1.0f;
        
        while (timer < duration)
        {
            // 씬 전환 중에는 코루틴 중단
            if (SceneManager.GetActiveScene().name == "")
            {
                if (loadingScreen != null)
                {
                    Destroy(loadingScreen);
                }
                yield break;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        try
        {
            // SaveSystem에서 데이터 로드
            (SaveData loadedData, Map loadedMap) = saveSystem.Load();
            
            if (loadedData != null)
            {
                // 로드된 데이터 적용
                playerData = loadedData;
                currentMap = loadedMap;
                
                // 캐싱된 데이터 업데이트
                nickName = playerStats.NickName;
                maxHealth = playerStats.MaxHp;
                health = playerStats.CurrHp;
                gold = playerStats.gold;
                characterPieceDataList = characterData.characterPieceDataList ?? new List<CharacterPieceData>();
                mailDataList = mailbox.mailDataList ?? new List<MailData>();
                
                // 플레이 타임 로드
                playTime = saveSystem.LoadPlayTime();
                timeSpan = TimeSpan.FromSeconds(playTime);
                
                // 카드 데이터 로드
                UpdateUserData();
                
                // UI 업데이트
                UpdateUserConfigData();
                OnPlayTimeChanged?.Invoke(timeSpan.ToString());
                
                Debug.Log("세이브 데이터 로드 완료");
            }
            else
            {
                Debug.LogWarning("로드할 세이브 데이터가 없습니다. 새 게임을 시작합니다.");
                ResetPlayerData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"로드 중 오류 발생: {e.Message}");
            ResetPlayerData();
        }
        finally
        {
            if (loadingScreen != null && this != null)
            {
                Destroy(loadingScreen);
            }
        }
    }
}
