using UnityEngine;
using UnityEngine.UI;
using CCGKit;

/// <summary>
/// 사운드 이펙트를 쉽게 사용할 수 있는 도우미 클래스
/// </summary>
public static class SoundEffectHelper
{
    /// <summary>
    /// 버튼에 클릭 사운드를 추가합니다.
    /// </summary>
    public static void AddButtonClickSound(Button button, string soundName = "card draw")
    {
        if (button == null)
        {
            Debug.LogError("버튼이 null입니다.");
            return;
        }

        button.onClick.AddListener(() => PlaySound(soundName));
    }
    
    /// <summary>
    /// 지정된 이름의 효과음을 재생합니다.
    /// </summary>
    public static void PlaySound(string soundName, float volume = 1.0f)
    {
        if (Parser_EffectSound.Instance != null)
        {
            Parser_EffectSound.Instance.PlaySoundEffect(soundName, volume);
        }
        else
        {
            Debug.LogWarning("Parser_EffectSound 인스턴스가 없습니다. 효과음을 재생할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 카드 효과 타입에 따른 효과음을 재생합니다.
    /// </summary>
    public static void PlayCardEffectSound(Parser_EffectSound.CardEffectType effectType, float volume = 1.0f)
    {
        if (Parser_EffectSound.Instance != null)
        {
            Parser_EffectSound.Instance.PlayEffectTypeSound(effectType, volume);
        }
        else
        {
            PlaySound("card draw", volume);
        }
    }
    
    /// <summary>
    /// 카드 타입에 따른 효과음을 재생합니다.
    /// </summary>
    public static void PlayCardTypeSound(object cardType, float volume = 1.0f)
    {
        if (Parser_EffectSound.Instance != null)
        {
            Parser_EffectSound.Instance.PlayCardTypeSound(cardType, volume);
        }
        else
        {
            // 기본 카드 타입 효과음
            PlaySound("card draw", volume);
        }
    }
    
    /// <summary>
    /// 공격력에 따른 효과음을 재생합니다.
    /// </summary>
    public static void PlayAttackSound(int attackPower, float volume = 1.0f)
    {
        if (Parser_EffectSound.Instance != null)
        {
            Parser_EffectSound.Instance.PlayAttackPowerSound(attackPower, volume);
        }
        else
        {
            // 기본 공격 효과음
            PlaySound("hitting1", volume);
        }
    }
    
    /// <summary>
    /// 특수 효과에 따른 효과음을 재생합니다.
    /// </summary>
    public static void PlaySpecialEffectSound(string effectName, float volume = 1.0f)
    {
        if (Parser_EffectSound.Instance != null)
        {
            Parser_EffectSound.Instance.PlaySpecialEffectSound(effectName, volume);
        }
        else
        {
            // 기본 효과음
            PlaySound("card draw", volume);
        }
    }
    
    /// <summary>
    /// 크리티컬 효과음을 재생합니다.
    /// </summary>
    public static void PlayCriticalSound(float volume = 1.0f)
    {
        PlaySpecialEffectSound("critical", volume);
    }
    
    /// <summary>
    /// 카드 효과 타입에 따른 효과음 이름을 반환합니다.
    /// </summary>
    private static string GetSoundNameForEffect(CardEffectType effectType)
    {
        switch (effectType)
        {
            case CardEffectType.Damage:
                return "hitting1";
                
            case CardEffectType.Shield:
                return "defense1";
                
            case CardEffectType.Draw:
                return "card draw";
                
            case CardEffectType.Heal:
                return "heal sound";
                
            case CardEffectType.Poison:
                return "posion debuff";
                
            case CardEffectType.Weak:
                return "etc debuff";
                
            case CardEffectType.Vulnerable:
                return "etc debuff";
                
            case CardEffectType.Strength:
                return "buff1";
                
            case CardEffectType.Critical:
                return "cirtical attack";
                
            default:
                return "";
        }
    }
}

/// <summary>
/// 카드 효과 타입 열거형
/// </summary>
public enum CardEffectType
{
    Damage,
    Shield,
    Draw,
    Heal,
    Poison,
    Weak,
    Vulnerable,
    Strength,
    Critical
} 