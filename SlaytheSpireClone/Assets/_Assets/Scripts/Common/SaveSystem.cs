using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using CCGKit;

// 배열 이름과 에디터상의 문자는 반드시 같아야 한다!!
public enum SaveCharacterIndex
{
    Galahad,    // 갤러해드. 기본값
    Lancelot,
    Percival,
    Max
}

public class CharacterGachaData
{
    public CharacterGachaData()
    {
        overlapCount = 0;
        characterIndex = SaveCharacterIndex.Galahad;
    }

    public CharacterGachaData(SaveCharacterIndex characterIndex)
    {
        this.characterIndex = characterIndex;
    }

    // 중복된 캐릭터 카운트. 중복 1당 hp 1씩 증가
    public int overlapCount;

    public SaveCharacterIndex characterIndex;
}

[Serializable]
public class CharacterPieceData
{
    // 캐릭터 조각 획득 현황
    public CharacterPieceData(SaveCharacterIndex characterIndex, int count)
    {
        this.characterIndex = characterIndex;
        this.count = count;
    }
    public static CharacterPieceData operator +(CharacterPieceData a, CharacterPieceData b)
    {
        if (a.characterIndex != b.characterIndex)
        {
            throw new ArgumentException("두 CharacterPieceData의 characterIndex가 다릅니다.");
        }
        return new CharacterPieceData(a.characterIndex, a.count + b.count);
    }
    public static CharacterPieceData operator -(CharacterPieceData a, CharacterPieceData b)
    {
        if (a.characterIndex != b.characterIndex)
        {
            throw new ArgumentException("두 CharacterPieceData의 characterIndex가 다릅니다.");
        }
        return new CharacterPieceData(a.characterIndex, a.count - b.count);
    }
    public static CharacterPieceData operator *(CharacterPieceData a, int b)
    {
        return new CharacterPieceData(a.characterIndex, a.count * b);
    }
    public static CharacterPieceData operator /(CharacterPieceData a, int b)
    {
        return new CharacterPieceData(a.characterIndex, a.count / b);
    }
    public void SetInfo(SaveCharacterIndex characterIndex, int count)
    {
        this.characterIndex = characterIndex;
        this.count = count;
    }

    public SaveCharacterIndex characterIndex;
    public int count;
}

[Serializable]
public class MailData
{
    public MailData(string title, string content, bool isRead, int giftGold)
    {
        this.title = title;
        this.content = content;
        this.isRead = isRead;
        this.giftGold = giftGold;
    }

    public string title;
    public string content;
    public bool isRead;
    public int giftGold;
}

public interface ISaveSystem
{
    bool IsSaveDataExist();
    (SaveData, Map) Load();
    void Save(SaveData saveData, Map map);
    Map LoadMap();
    void SaveMap(Map map);
    float LoadPlayTime();
}

public class SaveSystem : Singleton<SaveSystem>
{
    private readonly string saveDataPrefKey = "save";
    private readonly string mapPrefKey = "map";
    private readonly string playTimePrefKey = "playTime";

    // GameManager에서만 접근 가능한 인터페이스
    internal interface ISaveSystem
    {
        void SaveGameData(SaveData saveData);
        SaveData LoadGameData();
        void SaveMap(Map map);
        Map LoadMap();
        bool IsSaveDataExist();
        (SaveData, Map) Load();
        void Save(SaveData saveData, Map map);
        void SavePlayTime(float playTime, string timeSpanString);
        float LoadPlayTime();
    }

    // GameManager 전용 인터페이스 구현체 반환
    internal ISaveSystem GetManagerInterface()
    {
        return new SaveSystemManagerInterface(this);
    }

    // GameManager 전용 인터페이스 구현
    private class SaveSystemManagerInterface : ISaveSystem
    {
        private readonly SaveSystem saveSystem;

        public SaveSystemManagerInterface(SaveSystem saveSystem)
        {
            this.saveSystem = saveSystem;
        }

        public void SaveGameData(SaveData saveData) => saveSystem.SaveGameDataInternal(saveData);
        public SaveData LoadGameData() => saveSystem.LoadGameDataInternal();
        public void SaveMap(Map map) => saveSystem.SaveMapInternal(map);
        public Map LoadMap() => saveSystem.LoadMapInternal();
        public bool IsSaveDataExist() => saveSystem.IsSaveDataExistInternal();
        public (SaveData, Map) Load() => saveSystem.Load();
        public void Save(SaveData saveData, Map map) => saveSystem.Save(saveData, map);
        public void SavePlayTime(float playTime, string timeSpanString) => 
            saveSystem.SavePlayTime(playTime, timeSpanString);
        public float LoadPlayTime() => saveSystem.LoadPlayTime();
    }

    // 내부 구현 메서드들
    private void SaveGameDataInternal(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(saveDataPrefKey, json);
        PlayerPrefs.Save();
    }

    private SaveData LoadGameDataInternal()
    {
        if (PlayerPrefs.HasKey(saveDataPrefKey))
        {
            string json = PlayerPrefs.GetString(saveDataPrefKey);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return new SaveData();
    }

    private void SaveMapInternal(Map map)
    {
        var json = JsonUtility.ToJson(map, true);
        PlayerPrefs.SetString(mapPrefKey, json);
        PlayerPrefs.Save();
    }

    private Map LoadMapInternal()
    {
        if (PlayerPrefs.HasKey(mapPrefKey))
        {
            var json = PlayerPrefs.GetString(mapPrefKey);
            return JsonUtility.FromJson<Map>(json);
        }
        return null;
    }

    private bool IsSaveDataExistInternal()
    {
        return PlayerPrefs.HasKey(saveDataPrefKey);
    }

    // GameManager에서만 호출 가능한 저장 메서드
    internal void Save(SaveData saveData, Map map = null)
    {
        SaveGameDataInternal(saveData);
        if (map != null)
        {
            SaveMapInternal(map);
        }
    }

    // GameManager에서만 호출 가능한 로드 메서드
    internal (SaveData, Map) Load()
    {
        return (LoadGameDataInternal(), LoadMapInternal());
    }

    // 캐릭터 저장
    public void SetSaveCharacterData(SaveCharacterIndex collectedCharacter)
    {
        SaveData saveData = LoadGameDataInternal();
        if (saveData == null)
        {
            saveData = new SaveData();
        }

        if (saveData.characterData.SaveCharacterIndexList == null)
        {
            saveData.characterData.SaveCharacterIndexList = new List<SaveCharacterIndex>();
        }

        // 기본 캐릭터 추가
        if (!saveData.characterData.SaveCharacterIndexList.Contains(SaveCharacterIndex.Galahad))
        {
            saveData.characterData.SaveCharacterIndexList.Add(SaveCharacterIndex.Galahad);
        }

        // 새 캐릭터 추가
        if (collectedCharacter != SaveCharacterIndex.Max && 
            !saveData.characterData.SaveCharacterIndexList.Contains(collectedCharacter))
        {
            saveData.characterData.SaveCharacterIndexList.Add(collectedCharacter);
        }

        SaveGameDataInternal(saveData);
    }

    // 중복 캐릭터 저장
    public void SetSaveOverlapCharacterData(CharacterGachaData collectedCharacter)
    {
        SaveData saveData = LoadGameDataInternal();
        saveData.characterData.SaveCharacterIndexList.Add(collectedCharacter.characterIndex);
        saveData.characterData.charGachaData.overlapCount++;
        SaveGameDataInternal(saveData);
    }

    // 카드 저장
    public void SetSaveCardData(int id)
    {
        SaveData saveData = LoadGameDataInternal();
        saveData.deckData.Deck.Add(id);
        SaveGameDataInternal(saveData);
    }

    public void SetSaveMailData(MailData mailData)
    {
        SaveData saveData = LoadGameDataInternal();
        saveData.mailbox.mailDataList.Add(mailData);
        SaveGameDataInternal(saveData);
    }
    
    public void ResetSaveMailData()
    {
        SaveData saveData = LoadGameDataInternal();
        saveData.mailbox.mailDataList.Clear();
        SaveGameDataInternal(saveData);
    }

    public void ResetSaveCharacterData()
    {
        SaveData saveData = LoadGameDataInternal();
        saveData.characterData.SaveCharacterIndexList.Clear();
        SaveGameDataInternal(saveData);
    }
    public void SetCurrentCharacterIndex(SaveCharacterIndex characterIndex)
    {
        try
        {
            SaveData saveData = LoadGameDataInternal();
            if (saveData == null)
            {
                saveData = new SaveData();
            }

            saveData.characterData.currentCharacterIndex = characterIndex;

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

            saveData.stats.MaxHp = defaultMaxHealth;
            saveData.stats.CurrHp = defaultHealth;

            SaveGameDataInternal(saveData);
        }
        catch (Exception e)
        {
            Debug.LogError($"캐릭터 인덱스 설정 실패: {e.Message}");
        }
    }
    public SaveCharacterIndex GetCurrentCharacterIndex()
    {
        SaveData saveData = LoadGameDataInternal();
        return saveData.characterData.currentCharacterIndex;
    }

    public void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F1))
        {
            SaveData saveData = LoadGameDataInternal();
            saveData.progress.IsGetStartRelic = true;
            SaveGameDataInternal(saveData);
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            SaveData saveData = LoadGameDataInternal();
            if(saveData.progress.IsGetStartRelic)
            {
                saveData.progress.IsGetStartRelic = false;
                SaveGameDataInternal(saveData);
            }
        }
        #endif
    }

    // 저장된 캐릭터 인덱스 목록을 반환하는 함수 추가
    public List<SaveCharacterIndex> GetSavedCharacterIndexList()
    {
        SaveData saveData = LoadGameDataInternal();
        return saveData.characterData.SaveCharacterIndexList;
    }

    // 플레이 타임 저장 메서드 추가
    public void SavePlayTime(float playTime, string timeSpanString)
    {
        try
        {
            PlayerPrefs.SetFloat(playTimePrefKey, playTime);
            PlayerPrefs.Save();
            Debug.Log($"플레이 타임 저장: {timeSpanString}");
        }
        catch (Exception e)
        {
            Debug.LogError($"플레이 타임 저장 중 오류: {e.Message}");
        }
    }

    public float LoadPlayTime()
    {
        try
        {
            if (PlayerPrefs.HasKey(playTimePrefKey))
            {
                return PlayerPrefs.GetFloat(playTimePrefKey);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"플레이 타임 로드 중 오류: {e.Message}");
        }
        return 0f;
    }
}
