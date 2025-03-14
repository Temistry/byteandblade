using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 카드 효과 패턴을 번역하는 유틸리티 클래스
/// </summary>
public static class CardEffectPatternTranslator
{
    // 효과 패턴 정규식
    private static readonly Regex DealDamagePattern = new Regex(@"Deal (\d+) damage", RegexOptions.Compiled);
    private static readonly Regex GainShieldPattern = new Regex(@"Gain (\d+) Shield", RegexOptions.Compiled);
    private static readonly Regex DrawCardsPattern = new Regex(@"Draw (\d+) Card(s?)", RegexOptions.Compiled);
    private static readonly Regex GainHpPattern = new Regex(@"Gain (\d+) HP", RegexOptions.Compiled);
    private static readonly Regex ApplyPoisonPattern = new Regex(@"Apply (\d+) Poison", RegexOptions.Compiled);
    private static readonly Regex ApplyWeakPattern = new Regex(@"Apply (\d+) Weak", RegexOptions.Compiled);
    private static readonly Regex ApplyVulnerablePattern = new Regex(@"Apply (\d+) Vulnerable", RegexOptions.Compiled);
    private static readonly Regex ApplyStrengthPattern = new Regex(@"Apply (\d+) Strength", RegexOptions.Compiled);
    private static readonly Regex GainManaPattern = new Regex(@"Gain (\d+) Mana", RegexOptions.Compiled);
    private static readonly Regex CloneCardPattern = new Regex(@"Clone (\d+) card", RegexOptions.Compiled);
    
    // 효과 패턴 번역 템플릿
    private static readonly Dictionary<Regex, string> KoreanPatternTemplates = new Dictionary<Regex, string>
    {
        { DealDamagePattern, "{0} 데미지 공격" },
        { GainShieldPattern, "{0} 방어도 획득" },
        { DrawCardsPattern, "카드 {0}장 뽑기" },
        { GainHpPattern, "체력 {0} 회복" },
        { ApplyPoisonPattern, "독 {0} 부여" },
        { ApplyWeakPattern, "약화 {0} 부여" },
        { ApplyVulnerablePattern, "취약 {0} 부여" },
        { ApplyStrengthPattern, "힘 {0} 부여" },
        { GainManaPattern, "마나 {0} 획득" },
        { CloneCardPattern, "카드 {0}장 복제" }
    };
    
    private static readonly Dictionary<Regex, string> EnglishPatternTemplates = new Dictionary<Regex, string>
    {
        { DealDamagePattern, "Deal {0} Damage" },
        { GainShieldPattern, "Gain {0} Shield" },
        { DrawCardsPattern, "Draw {0} Card{1}" },
        { GainHpPattern, "Heal {0} HP" },
        { ApplyPoisonPattern, "Apply {0} Poison" },
        { ApplyWeakPattern, "Apply {0} Weak" },
        { ApplyVulnerablePattern, "Apply {0} Vulnerable" },
        { ApplyStrengthPattern, "Apply {0} Strength" },
        { GainManaPattern, "Gain {0} Mana" },
        { CloneCardPattern, "Clone {0} card" }
    };
    
    /// <summary>
    /// 효과 설명을 한국어로 번역합니다.
    /// </summary>
    public static string TranslateEffectToKorean(string effectDescription)
    {
        foreach (var pattern in KoreanPatternTemplates.Keys)
        {
            Match match = pattern.Match(effectDescription);
            if (match.Success)
            {
                string template = KoreanPatternTemplates[pattern];
                
                if (pattern == DrawCardsPattern)
                {
                    // Draw Cards 패턴은 복수형 처리가 필요
                    string count = match.Groups[1].Value;
                    return string.Format(template, count);
                }
                else
                {
                    string value = match.Groups[1].Value;
                    return string.Format(template, value);
                }
            }
        }
        
        return effectDescription;
    }
    
    /// <summary>
    /// 효과 설명을 영어로 번역합니다.
    /// </summary>
    public static string TranslateEffectToEnglish(string effectDescription)
    {
        foreach (var pattern in EnglishPatternTemplates.Keys)
        {
            Match match = pattern.Match(effectDescription);
            if (match.Success)
            {
                string template = EnglishPatternTemplates[pattern];
                
                if (pattern == DrawCardsPattern)
                {
                    // Draw Cards 패턴은 복수형 처리가 필요
                    string count = match.Groups[1].Value;
                    string plural = count == "1" ? "" : "s";
                    return string.Format(template, count, plural);
                }
                else
                {
                    string value = match.Groups[1].Value;
                    return string.Format(template, value);
                }
            }
        }
        
        return effectDescription;
    }
    
    /// <summary>
    /// 효과 설명에서 패턴 키를 추출합니다.
    /// </summary>
    public static string ExtractPatternKey(string effectDescription)
    {
        foreach (var pattern in EnglishPatternTemplates.Keys)
        {
            Match match = pattern.Match(effectDescription);
            if (match.Success)
            {
                if (pattern == DrawCardsPattern)
                {
                    // Draw Cards 패턴은 복수형 처리가 필요
                    string count = match.Groups[1].Value;
                    string plural = count == "1" ? "" : "s";
                    return string.Format("Draw {0} Card{1}", "{0}", plural);
                }
                else if (pattern == DealDamagePattern)
                {
                    return "Deal {0} damage";
                }
                else if (pattern == GainShieldPattern)
                {
                    return "Gain {0} Shield";
                }
                else if (pattern == GainHpPattern)
                {
                    return "Gain {0} HP";
                }
                else if (pattern == ApplyPoisonPattern)
                {
                    return "Apply {0} Poison";
                }
                else if (pattern == ApplyWeakPattern)
                {
                    return "Apply {0} Weak";
                }
                else if (pattern == ApplyVulnerablePattern)
                {
                    return "Apply {0} Vulnerable";
                }
                else if (pattern == ApplyStrengthPattern)
                {
                    return "Apply {0} Strength";
                }
                else if (pattern == GainManaPattern)
                {
                    return "Gain {0} Mana";
                }
                else if (pattern == CloneCardPattern)
                {
                    return "Clone {0} card";
                }
            }
        }
        
        return effectDescription;
    }
} 