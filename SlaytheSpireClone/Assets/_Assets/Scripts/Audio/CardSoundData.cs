using System.Collections.Generic;
using UnityEngine;
using CCGKit;

/// <summary>
/// 카드 타입과 효과음을 매핑하는 스크립터블 오브젝트입니다.
/// </summary>
[CreateAssetMenu(fileName = "CardSoundData", menuName = "Audio/Card Sound Data")]
public class CardSoundData : ScriptableObject
{
    // 각 카드 타입별 기본 효과음
    [System.Serializable]
    public class CardTypeSoundEntry
    {
        public CardType cardType;            // 카드 타입
        public string soundName;             // 기본 효과음 이름 (어드레서블 ID)
        public List<string> variations = new List<string>(); // 효과음 변형들 (여러 효과음 중 랜덤 선택용)
    }
    
    // 공격력 범위별 효과음
    [System.Serializable]
    public class AttackPowerSoundEntry
    {
        public int minPower;            // 최소 공격력
        public int maxPower;            // 최대 공격력
        public string soundName;        // 기본 효과음 이름 (어드레서블 ID)
        public List<string> variations = new List<string>(); // 효과음 변형들
    }
    
    // 효과 타입별 효과음
    [System.Serializable]
    public class EffectTypeSoundEntry
    {
        public Parser_EffectSound.CardEffectType effectType;   // 효과 타입
        public string soundName;            // 기본 효과음 이름 (어드레서블 ID)
        public List<string> variations = new List<string>(); // 효과음 변형들
    }
    
    // 특수 효과 타입별 효과음 (크리티컬, 약점 공격 등)
    [System.Serializable]
    public class SpecialEffectSoundEntry
    {
        public string effectName;           // 특수 효과 이름 (예: "Critical", "Weak", "Vulnerable")
        public string soundName;            // 기본 효과음 이름 (어드레서블 ID)
        public List<string> variations = new List<string>(); // 효과음 변형들
    }
    
    // 각 매핑 리스트
    public List<CardTypeSoundEntry> cardTypeSounds = new List<CardTypeSoundEntry>();
    public List<AttackPowerSoundEntry> attackPowerSounds = new List<AttackPowerSoundEntry>();
    public List<EffectTypeSoundEntry> effectTypeSounds = new List<EffectTypeSoundEntry>();
    public List<SpecialEffectSoundEntry> specialEffectSounds = new List<SpecialEffectSoundEntry>();
    
    // 기본 효과음 이름 (매핑이 없을 경우)
    public string defaultSound = "card draw";
    
    /// <summary>
    /// 카드 타입에 해당하는 효과음 이름을 가져옵니다.
    /// </summary>
    public string GetSoundForCardType(CardType cardType, bool useVariation = true)
    {
        // 해당 카드 타입의 효과음 찾기
        CardTypeSoundEntry entry = cardTypeSounds.Find(e => e.cardType == cardType);
        
        if (entry != null)
        {
            // 변형 효과음 사용 여부 확인
            if (useVariation && entry.variations.Count > 0)
            {
                // 변형 효과음 중 랜덤 선택
                int randomIndex = Random.Range(0, entry.variations.Count);
                return entry.variations[randomIndex];
            }
            
            return entry.soundName;
        }
        
        // 매핑이 없으면 기본 효과음 반환
        return defaultSound;
    }
    
    /// <summary>
    /// 공격력에 해당하는 효과음 이름을 가져옵니다.
    /// </summary>
    public string GetSoundForAttackPower(int attackPower, bool useVariation = true)
    {
        // 해당 범위의 공격력 효과음 찾기
        AttackPowerSoundEntry entry = attackPowerSounds.Find(e => 
            attackPower >= e.minPower && attackPower <= e.maxPower);
        
        if (entry != null)
        {
            // 변형 효과음 사용 여부 확인
            if (useVariation && entry.variations.Count > 0)
            {
                // 변형 효과음 중 랜덤 선택
                int randomIndex = Random.Range(0, entry.variations.Count);
                return entry.variations[randomIndex];
            }
            
            return entry.soundName;
        }
        
        // 매핑이 없으면 기본 효과음 반환
        return defaultSound;
    }
    
    /// <summary>
    /// 효과 타입에 해당하는 효과음 이름을 가져옵니다.
    /// </summary>
    public string GetSoundForEffectType(Parser_EffectSound.CardEffectType effectType, bool useVariation = true)
    {
        // 해당 효과 타입의 효과음 찾기
        EffectTypeSoundEntry entry = effectTypeSounds.Find(e => e.effectType == effectType);
        
        if (entry != null)
        {
            // 변형 효과음 사용 여부 확인
            if (useVariation && entry.variations.Count > 0)
            {
                // 변형 효과음 중 랜덤 선택
                int randomIndex = Random.Range(0, entry.variations.Count);
                return entry.variations[randomIndex];
            }
            
            return entry.soundName;
        }
        
        // 매핑이 없으면 기본 효과음 반환
        return defaultSound;
    }
    
    /// <summary>
    /// 특수 효과에 해당하는 효과음 이름을 가져옵니다.
    /// </summary>
    public string GetSoundForSpecialEffect(string effectName, bool useVariation = true)
    {
        // 해당 특수 효과의 효과음 찾기
        SpecialEffectSoundEntry entry = specialEffectSounds.Find(e => e.effectName == effectName);
        
        if (entry != null)
        {
            // 변형 효과음 사용 여부 확인
            if (useVariation && entry.variations.Count > 0)
            {
                // 변형 효과음 중 랜덤 선택
                int randomIndex = Random.Range(0, entry.variations.Count);
                return entry.variations[randomIndex];
            }
            
            return entry.soundName;
        }
        
        // 매핑이 없으면 기본 효과음 반환
        return defaultSound;
    }
} 