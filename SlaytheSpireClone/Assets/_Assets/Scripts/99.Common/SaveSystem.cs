using UnityEngine;
using System;
using System.Collections.Generic;


public enum SaveCharacterIndex
{
    Standard,
    Galahad,
    Lancelot,
    Percival,
}

[Serializable]
public class SaveData
{
    public int Hp;
    public int Shield;
    public List<int> Deck = new List<int>();

    public List<string> CollectedCards = new List<string>(); // 수집한 카드 목록
    public string CollectedCharacter; // 수집한 캐릭터

    // 저장된 캐릭터들
    public List<SaveCharacterIndex> SaveCharacterIndexList = new List<SaveCharacterIndex>();

    // 현재 캐릭터
    public SaveCharacterIndex CurrentCharacterIndex = SaveCharacterIndex.Standard;

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

    // 카드와 캐릭터 저장 메서드 추가
    public void SetSaveCollectedData(List<string> collectedCards, string collectedCharacter)
    {
        SaveData saveData = LoadGameData();
        saveData.CollectedCards = collectedCards;
        saveData.CollectedCharacter = collectedCharacter;
        saveData.SaveCharacterIndexList.Add(collectedCharacter.ToString());
        saveData.CurrentCharacterIndex = collectedCharacter.ToString();
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
        saveData.CurrentCharacterIndex = characterIndex;
        SaveGameData(saveData);
    }
}
