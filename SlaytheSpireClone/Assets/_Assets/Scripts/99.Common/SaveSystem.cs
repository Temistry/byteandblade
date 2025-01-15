using UnityEngine;
using System;
using System.Collections.Generic;


public enum SaveCharacterIndex
{
    Galahad,
    Lancelot,
    Percival,
    Max,
    None
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
public class SaveData
{
    public int Hp;
    public int Shield;

    public List<int> Deck = new List<int>();

    public List<string> CollectedCards = new List<string>(); // 수집한 카드 목록

    // 저장된 캐릭터들
    public List<SaveCharacterIndex> SaveCharacterIndexList = new List<SaveCharacterIndex>();

    // 캐릭터 뽑기 데이터. 중복 데이터 처리 포함
    public CharacterGachaData charGachaData = new CharacterGachaData();

    // 현재 플레이어블 캐릭터
    public SaveCharacterIndex currentCharacterIndex = SaveCharacterIndex.None;
}


public class SaveSystem : Singleton<SaveSystem>
{
    private readonly string saveDataPrefKey = "save";

    public void SaveGameData(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(saveDataPrefKey, json);
        PlayerPrefs.Save();
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
    public void SetSaveCardData(string collectedCard)
    {
        SaveData saveData = LoadGameData();
        saveData.CollectedCards.Add(collectedCard);
        SaveGameData(saveData);
    }

    public void SetSaveCharacterIndex(SaveCharacterIndex characterIndex)
    {
        SaveData saveData = LoadGameData();
        saveData.SaveCharacterIndexList.Add(characterIndex);
        SaveGameData(saveData);
    }

    public void SetCurrentCharacterIndex(SaveCharacterIndex characterIndex)
    {
        SaveData saveData = LoadGameData();
        saveData.currentCharacterIndex = characterIndex;
        SaveGameData(saveData);
    }
}
