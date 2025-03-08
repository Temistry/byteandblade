using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CCGKit;


[Serializable]
public class SavePlayerStats
{
    public int MaxHp;
    public int CurrHp;
    public int Shield;
    public int gold;
    public string NickName;
}

[Serializable]
public class SaveCharacterData
{
    public SaveCharacterIndex currentCharacterIndex;
    public List<SaveCharacterIndex> SaveCharacterIndexList = new List<SaveCharacterIndex>();
    public CharacterGachaData charGachaData = new CharacterGachaData();
    public List<CharacterPieceData> characterPieceDataList = new List<CharacterPieceData>();
}

[Serializable]
public class DeckData
{
    public List<int> Deck = new List<int>();  // 카드 ID 목록
}

[Serializable]
public class MailboxData
{
    public List<MailData> mailDataList = new List<MailData>();
}

[Serializable]
public class GameProgressData
{
    public bool IsBossCleared;  // 보스 클리어 여부
    public int BossClearCount;  // 보스 클리어 횟수
    public Map map;  // 맵 데이터도 여기에 저장
}

[Serializable]
public class SaveData
{
    public SavePlayerStats stats = new SavePlayerStats();
    public SaveCharacterData characterData = new SaveCharacterData();
    public DeckData deckData = new DeckData();
    public MailboxData mailbox = new MailboxData();
    public GameProgressData progress = new GameProgressData();
}


