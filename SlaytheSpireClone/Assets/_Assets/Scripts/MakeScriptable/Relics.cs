using UnityEngine;

[CreateAssetMenu(fileName = "RelicData", menuName = "Game Data/Relic")]
public class RelicData : ScriptableObject
{
    public int relicId;          // 유물의 고유 키값
    public string relicName;     // 유물 이름
    public string description;   // 유물 설명
    public Sprite icon;          // 유물 아이콘
    
    [Header("효과")]
    public int healthBonus;      // 체력 보너스
    public int attackBonus;      // 공격력 보너스
    public int defenseBonus;     // 방어력 보너스
    
    [Header("특수 효과")]
    public bool isStartingRelic; // 시작 유물 여부
    public RelicRarity rarity;   // 유물 희귀도
}

public enum RelicRarity
{
    Common,
    Uncommon,
    Rare,
    Boss
}