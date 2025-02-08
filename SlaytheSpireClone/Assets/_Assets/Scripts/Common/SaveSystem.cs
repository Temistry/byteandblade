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


[Serializable]
public class SaveData
{
    public string NickName;
    public int MaxHp;
    public int CurrHp;
    public int Shield;
    public int gold;


    public List<int> Deck = new List<int>();// 수집한 카드 목록

    public bool IsGetStartRelic;

    // 저장된 캐릭터들
    public List<SaveCharacterIndex> SaveCharacterIndexList = new List<SaveCharacterIndex>();

    // 캐릭터 뽑기 데이터. 중복 데이터 처리 포함
    public CharacterGachaData charGachaData = new CharacterGachaData();

    // 현재 플레이어블 캐릭터
    public SaveCharacterIndex currentCharacterIndex;
    public Map map;

    public List<MailData> mailDataList = new List<MailData>();
}


public class SaveSystem : Singleton<SaveSystem>
{

    private readonly string saveDataPrefKey = "save";
    private readonly string mapPrefKey = "map";

    public void SaveGameData(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(saveDataPrefKey, json);
        PlayerPrefs.Save();
    }

    // 저장된 데이터가 있는지 확인
    public bool IsSaveDataExist()
    {
        return PlayerPrefs.HasKey(saveDataPrefKey);
    }

    public SaveData LoadGameData()
    {
        if (PlayerPrefs.HasKey(saveDataPrefKey))
        {
            string json = PlayerPrefs.GetString(saveDataPrefKey);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return new SaveData(); // 데이터가 없으면 새 인스턴스 반환
    }

    // 캐릭터 저장
    public void SetSaveCharacterData(SaveCharacterIndex collectedCharacter)
    {
        SaveData saveData = LoadGameData();
        saveData.SaveCharacterIndexList.Add(collectedCharacter);
        saveData.charGachaData.characterIndex = collectedCharacter;


        var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;

        // 캐릭터 기본 덱 추가
        var template = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[(int)collectedCharacter]).Result;
        foreach (var entry in template.StartingDeck.Entries)
        {
            saveData.Deck.Add(entry.Card.Id);
            Debug.Log($"캐릭터 기본 덱 추가 : {entry.Card.Id}");
        }

        SaveGameData(saveData);
        Debug.Log($"캐릭터 저장 완료 : {collectedCharacter.ToString()}");
    }

    // 중복 캐릭터 저장
    public void SetSaveOverlapCharacterData(CharacterGachaData collectedCharacter)
    {
        SaveData saveData = LoadGameData();
        saveData.SaveCharacterIndexList.Add(collectedCharacter.characterIndex);
        saveData.charGachaData.overlapCount++;  // 중복 카운트 증가
        SaveGameData(saveData);
        Debug.Log($"중복 캐릭터 저장 완료 : {collectedCharacter.characterIndex.ToString()}");
    }

    // 카드 저장
    public void SetSaveCardData(int id)
    {
        SaveData saveData = LoadGameData();
        saveData.Deck.Add(id);
        SaveGameData(saveData);
    }

    public void SetSaveMailData(MailData mailData)
    {
        SaveData saveData = LoadGameData();
        saveData.mailDataList.Add(mailData);
        SaveGameData(saveData);
    }
    
    public void ResetSaveMailData()
    {
        SaveData saveData = LoadGameData();
        saveData.mailDataList.Clear();
        SaveGameData(saveData);
    }

    public void SetCurrentCharacterIndex(SaveCharacterIndex characterIndex)
    {
        // 현재 캐릭터 설정
        SaveData saveData = LoadGameData();
        saveData.currentCharacterIndex = characterIndex;

        // 현재 캐릭터의 스펙 설정
        var characterTemplateList = Parser_CharacterList.GetInstance().AllcharacterTemplateList;
        var template = Addressables.LoadAssetAsync<HeroTemplate>(characterTemplateList[(int)characterIndex]).Result;
        saveData.MaxHp = template.MaxHealth;
        saveData.CurrHp = template.Health;

        SaveGameData(saveData);
    }

    public void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F1))
        {
            SaveData saveData = LoadGameData();
            saveData.IsGetStartRelic = true;
            SaveGameData(saveData);
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            SaveData saveData = LoadGameData();
            if(saveData.IsGetStartRelic)
            {
                saveData.IsGetStartRelic = false;
                SaveGameData(saveData);
            }
        }
        #endif
    }

    // 맵 저장
    public void SaveMap(Map map)
    {
        var json = JsonUtility.ToJson(map, true);
        PlayerPrefs.SetString(mapPrefKey, json);
        PlayerPrefs.Save();
    }

    // 맵 로드
    public Map LoadMap()
    {
        if (PlayerPrefs.HasKey(mapPrefKey))
        {
            var json = PlayerPrefs.GetString(mapPrefKey);
            return JsonUtility.FromJson<Map>(json);
        }
        return null; // 맵이 없으면 null 반환
    }
}
