using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;

public class LanguageManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static LanguageManager instance;
    
    // 지원하는 언어 종류
    public enum Language
    {
        English,
        Korean
    }

    // 현재 선택된 언어
    private Language currentLanguage = Language.English;

    // 언어별 단어 사전
    private Dictionary<string, Dictionary<Language, string>> wordDictionary = new Dictionary<string, Dictionary<Language, string>>();

    // 언어 변경 이벤트
    public static event Action<Language> OnLanguageChanged;

    // TextAsset 관련 코드 추가

    // 언어 파일 경로 (Resources 폴더 내)
    private const string LANGUAGE_FILE_PATH = "Languages/";

    // 언어별 파일 이름
    private static readonly Dictionary<Language, string> languageFiles = new Dictionary<Language, string>
    {
        { Language.English, "english" },
        { Language.Korean, "korean" }
    };

    /// <summary>
    /// 싱글톤 인스턴스 접근자
    /// </summary>
    public static LanguageManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LanguageManager");
                instance = go.AddComponent<LanguageManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    /// <summary>
    /// 현재 설정된 언어를 반환합니다.
    /// </summary>
    public Language CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            if (currentLanguage != value)
            {
                currentLanguage = value;
                OnLanguageChanged?.Invoke(currentLanguage);
                
                // 언어 설정 저장
                PlayerPrefs.SetInt("Language", (int)currentLanguage);
                PlayerPrefs.Save();
                
                Debug.Log($"언어가 변경되었습니다: {currentLanguage}");
            }
        }
    }

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 저장된 언어 설정 불러오기
        if (PlayerPrefs.HasKey("Language"))
        {
            currentLanguage = (Language)PlayerPrefs.GetInt("Language");
        }
        
        // 언어 파일 로드 (하드코딩된 번역 대신 파일에서 로드)
        LoadAllLanguages();
    }

    /// <summary>
    /// 단어 사전에 새 단어를 추가합니다.
    /// </summary>
    public void AddWord(string key, Language language, string translation)
    {
        if (!wordDictionary.ContainsKey(key))
        {
            wordDictionary[key] = new Dictionary<Language, string>();
        }
        
        wordDictionary[key][language] = translation;
    }

    /// <summary>
    /// 단어를 현재 언어로 번역합니다.
    /// </summary>
    public string TranslateWord(string key)
    {
        if (wordDictionary.TryGetValue(key, out var translations) && 
            translations.TryGetValue(currentLanguage, out var translation))
        {
            return translation;
        }
        
        // 번역이 없으면 원본 반환
        return key;
    }

    /// <summary>
    /// 게임 텍스트를 현재 언어에 맞게 변환합니다.
    /// </summary>
    public string GetGameText(params object[] args)
    {
        if (args == null || args.Length == 0)
            return string.Empty;
            
        // 전체 문구 구성
        string fullPhrase = string.Join(" ", args);
        
        // 1. 패턴 번역 먼저 시도
        string patternTranslated = TranslateWithPattern(fullPhrase);
        if (patternTranslated != fullPhrase)
        {
            return patternTranslated;
        }
        
        // 2. 전체 문구 번역 시도
        string phraseTranslated = TranslateFullPhrase(fullPhrase);
        if (phraseTranslated != fullPhrase)
        {
            return phraseTranslated;
        }
        
        // 3. 언어별 처리
        if (currentLanguage == Language.English)
        {
            // 영어 모드: 단순히 인자들을 공백으로 연결
            return fullPhrase;
        }
        else if (currentLanguage == Language.Korean)
        {
            // 개별 단어 번역
            List<object> translatedArgs = new List<object>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is string strArg)
                {
                    // 띄어쓰기가 있는 문자열인 경우 각 단어를 개별적으로 번역
                    if (strArg.Contains(" "))
                    {
                        string[] words = strArg.Split(' ');
                        List<string> translatedWords = new List<string>();
                        
                        foreach (var word in words)
                        {
                            translatedWords.Add(TranslateWord(word));
                        }
                        
                        translatedArgs.Add(string.Join(" ", translatedWords));
                    }
                    else
                    {
                        translatedArgs.Add(TranslateWord(strArg));
                    }
                }
                else
                {
                    translatedArgs.Add(args[i]);
                }
            }
            
            // 번역된 인자들을 공백으로 연결
            return string.Join(" ", translatedArgs);
        }
        
        return fullPhrase;
    }

    /// <summary>
    /// 전체 문구를 번역합니다. 사전에 전체 문구가 있으면 해당 번역을 반환합니다.
    /// </summary>
    public string TranslateFullPhrase(string phrase)
    {
        // 전체 문구를 키로 사용하여 번역 시도
        return TranslateWord(phrase);
    }

    /// <summary>
    /// 패턴을 사용하여 번역합니다.
    /// </summary>
    public string TranslateWithPattern(string phrase)
    {
        // 카드 효과 패턴 번역기 참조 오류 수정
        // 직접 참조 대신 리플렉션을 사용하여 존재 여부 확인 후 호출
        try {
            // 여러 가능한 네임스페이스 시도
            Type translatorType = null;
            string[] possibleTypeNames = new[] {
                "CardEffectPatternTranslator",
                "Assets._Assets.Scripts.Editor.CardEffectPatternTranslator"
            };
            
            foreach (var typeName in possibleTypeNames)
            {
                translatorType = Type.GetType(typeName);
                if (translatorType != null) break;
            }
            
            // Assembly에서 직접 검색 시도
            if (translatorType == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var typeName in possibleTypeNames)
                    {
                        translatorType = assembly.GetType(typeName);
                        if (translatorType != null) break;
                    }
                    if (translatorType != null) break;
                }
            }
            
            if (translatorType != null)
            {
                // 메서드 정보 가져오기
                var method = translatorType.GetMethod("TranslateEffectToKorean", 
                    BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    // 메서드 호출
                    var result = method.Invoke(null, new object[] { phrase }) as string;
                    if (result != null && result != phrase)
                    {
                        return result;
                    }
                }
            }
        }
        catch (Exception) {
            // 리플렉션 호출 실패 시 무시하고 계속 진행
        }
        
        // 1. 숫자 하나를 포함하는 패턴 (예: "Deal 14 Damage")
        var regex1 = new System.Text.RegularExpressions.Regex(@"(\w+)\s+(\d+)\s+(\w+)");
        var match1 = regex1.Match(phrase);
        
        if (match1.Success)
        {
            string action = match1.Groups[1].Value;
            string number = match1.Groups[2].Value;
            string target = match1.Groups[3].Value;
            
            string pattern = $"{action} {{0}} {target}";
            string translatedPattern = TranslateWord(pattern);
            
            if (translatedPattern != pattern)
            {
                return string.Format(translatedPattern, number);
            }
        }
        
        // 2. 숫자 두 개를 포함하는 패턴 (예: "Deal 14 Damage and Gain 5 Block")
        var regex2 = new System.Text.RegularExpressions.Regex(@"(\w+)\s+(\d+)\s+(\w+)\s+and\s+(\w+)\s+(\d+)\s+(\w+)");
        var match2 = regex2.Match(phrase);
        
        if (match2.Success)
        {
            string action1 = match2.Groups[1].Value;
            string number1 = match2.Groups[2].Value;
            string target1 = match2.Groups[3].Value;
            string action2 = match2.Groups[4].Value;
            string number2 = match2.Groups[5].Value;
            string target2 = match2.Groups[6].Value;
            
            string pattern = $"{action1} {{0}} {target1} and {action2} {{1}} {target2}";
            string translatedPattern = TranslateWord(pattern);
            
            if (translatedPattern != pattern)
            {
                return string.Format(translatedPattern, number1, number2);
            }
        }
        
        return phrase;
    }

    /// <summary>
    /// 패턴 사전에 새 패턴을 추가합니다.
    /// </summary>
    public void AddPattern(string pattern, Language language, string translation)
    {
        AddWord(pattern, language, translation);
    }

    /// <summary>
    /// 텍스트 파일에서 언어 사전을 로드합니다.
    /// </summary>
    public void LoadDictionaryFromTextAsset(TextAsset textAsset)
    {
        if (textAsset == null)
            return;
            
        string[] lines = textAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length >= 3)
            {
                string key = parts[0].Trim();
                
                if (Enum.TryParse<Language>(parts[1].Trim(), out Language language))
                {
                    string translation = parts[2].Trim();
                    AddWord(key, language, translation);
                }
            }
        }
    }
    
    // 정적 헬퍼 메서드들 - 인스턴스 메서드를 쉽게 호출할 수 있게 함
    
    /// <summary>
    /// 현재 언어 설정을 변경합니다.
    /// </summary>
    public static void SetLanguage(Language language)
    {
        Instance.CurrentLanguage = language;
    }
    
    /// <summary>
    /// 현재 언어 설정을 반환합니다.
    /// </summary>
    public static Language GetCurrentLanguage()
    {
        return Instance.CurrentLanguage;
    }
    
    /// <summary>
    /// 게임 텍스트를 현재 언어에 맞게 변환합니다. (정적 버전)
    /// </summary>
    public static string GetText(params object[] args)
    {
        return Instance.GetGameText(args);
    }
    
    /// <summary>
    /// 단어를 현재 언어로 번역합니다. (정적 버전)
    /// </summary>
    public static string Translate(string key)
    {
        return Instance.TranslateWord(key);
    }
    
    /// <summary>
    /// 단어 사전에 새 단어를 추가합니다. (정적 버전)
    /// </summary>
    public static void AddTranslation(string key, Language language, string translation)
    {
        Instance.AddWord(key, language, translation);
    }
    
    /// <summary>
    /// 텍스트 파일에서 언어 사전을 로드합니다. (정적 버전)
    /// </summary>
    public static void LoadDictionary(TextAsset textAsset)
    {
        Instance.LoadDictionaryFromTextAsset(textAsset);
    }

    /// <summary>
    /// 모든 언어 파일을 로드합니다.
    /// </summary>
    public void LoadAllLanguageFiles()
    {
        foreach (var language in Enum.GetValues(typeof(Language)))
        {
            LoadLanguageFile((Language)language);
        }
        Debug.Log("모든 언어 파일이 로드되었습니다.");
    }

    /// <summary>
    /// 특정 언어의 파일을 로드합니다.
    /// </summary>
    public void LoadLanguageFile(Language language)
    {
        if (languageFiles.TryGetValue(language, out string fileName))
        {
            TextAsset textAsset = Resources.Load<TextAsset>(LANGUAGE_FILE_PATH + fileName);
            if (textAsset != null)
            {
                LoadLanguageFromTextAsset(textAsset, language);
                Debug.Log($"{language} 언어 파일이 로드되었습니다.");
            }
            else
            {
                Debug.LogWarning($"{language} 언어 파일을 찾을 수 없습니다: {LANGUAGE_FILE_PATH + fileName}");
            }
        }
    }

    /// <summary>
    /// TextAsset에서 특정 언어의 번역을 로드합니다.
    /// </summary>
    public void LoadLanguageFromTextAsset(TextAsset textAsset, Language language)
    {
        if (textAsset == null)
            return;
        
        string[] lines = textAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            // 주석 라인 무시
            if (line.StartsWith("#") || line.StartsWith("//"))
                continue;
            
            string[] parts = line.Split(',');
            if (parts.Length >= 3)  // 한국어, 영어 모두 포함
            {
                string key = parts[0].Trim();
                string koreanTranslation = parts[1].Trim();
                string englishTranslation = parts[2].Trim();
                
                // 한국어 번역 추가
                AddWord(key, Language.Korean, koreanTranslation);
                
                // 영어 번역 추가
                AddWord(key, Language.English, englishTranslation);
            }
            else if (parts.Length >= 2)  // 한국어만 포함
            {
                string key = parts[0].Trim();
                string koreanTranslation = parts[1].Trim();
                
                // 한국어 번역 추가
                AddWord(key, Language.Korean, koreanTranslation);
                
                // 영어는 키 그대로 사용
                AddWord(key, Language.English, key);
            }
        }
    }

    /// <summary>
    /// 현재 언어 파일을 다시 로드합니다.
    /// </summary>
    public void ReloadCurrentLanguage()
    {
        LoadLanguageFile(currentLanguage);
    }

    // 정적 헬퍼 메서드 추가
    public static void LoadAllLanguages()
    {
        Instance.LoadAllLanguageFiles();
    }

    public static void LoadLanguage(Language language)
    {
        Instance.LoadLanguageFile(language);
    }

    public static void ReloadLanguage()
    {
        Instance.ReloadCurrentLanguage();
    }

    // 정적 헬퍼 메서드 추가
    public static void AddPatternTranslation(string pattern, Language language, string translation)
    {
        Instance.AddPattern(pattern, language, translation);
    }
}
