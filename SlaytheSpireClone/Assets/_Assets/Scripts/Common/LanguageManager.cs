using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

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
        
        // 기본 단어 사전 초기화
        InitializeDefaultDictionary();

        LoadAllLanguages();
    }

    /// <summary>
    /// 기본 단어 사전을 초기화합니다.
    /// </summary>
    private void InitializeDefaultDictionary()
    {
        // 기본 단어 추가
        AddWord("Deal", Language.English, "Deal");
        AddWord("Deal", Language.Korean, "공격");
        
        AddWord("Damage", Language.English, "Damage");
        AddWord("Damage", Language.Korean, "데미지");
        
        AddWord("Gain", Language.English, "Gain");
        AddWord("Gain", Language.Korean, "획득");
        
        AddWord("Block", Language.English, "Block");
        AddWord("Block", Language.Korean, "방어도");
        
        AddWord("Draw", Language.English, "Draw");
        AddWord("Draw", Language.Korean, "뽑기");
        
        AddWord("Card", Language.English, "Card");
        AddWord("Card", Language.Korean, "카드");
        
        AddWord("Cards", Language.English, "Cards");
        AddWord("Cards", Language.Korean, "카드");

        AddWord("Reset", Language.English, "Reset");
        AddWord("Reset", Language.Korean, "초기화");

        AddWord("Yes", Language.English, "Yes");
        AddWord("Yes", Language.Korean, "예");

        AddWord("No", Language.English, "No");
        AddWord("No", Language.Korean, "아니오");

        AddWord("Cancel", Language.English, "Cancel");
        AddWord("Cancel", Language.Korean, "취소");

        AddWord("OK", Language.English, "OK");
        AddWord("OK", Language.Korean, "확인");

        AddWord("Back", Language.English, "Back");
        AddWord("Back", Language.Korean, "뒤로");

        AddWord("Exit", Language.English, "Exit");
        AddWord("Exit", Language.Korean, "종료");

        AddWord("Save", Language.English, "Save");
        AddWord("Save", Language.Korean, "저장");

        AddWord("Load", Language.English, "Load");
        AddWord("Load", Language.Korean, "불러오기");

        AddWord("Option", Language.English, "Option");
        AddWord("Option", Language.Korean, "설정");

        AddWord("Help", Language.English, "Help");
        AddWord("Help", Language.Korean, "도움말");

        AddWord("About", Language.English, "About");
        AddWord("About", Language.Korean, "정보");

        AddWord("Language", Language.English, "Language");
        AddWord("Language", Language.Korean, "언어");

        AddWord("English", Language.English, "English");
        AddWord("English", Language.Korean, "영어");

        AddWord("Korean", Language.English, "Korean");
        AddWord("Korean", Language.Korean, "한국어");

        AddWord("Player", Language.English, "Player");
        AddWord("Player", Language.Korean, "플레이어");

        AddWord("Data", Language.English, "Data");
        AddWord("Data", Language.Korean, "데이터");
        
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
            
        if (currentLanguage == Language.English)
        {
            // 영어 모드: 단순히 인자들을 공백으로 연결
            return string.Join(" ", args);
        }
        else if (currentLanguage == Language.Korean)
        {
            // 한국어 모드: 패턴에 따라 다르게 처리
            List<object> translatedArgs = new List<object>();
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is string strArg)
                {
                    // 먼저 통번역이 가능한 문자인지 검사
                    if (TranslateWord(strArg) != strArg)
                    {
                        translatedArgs.Add(TranslateWord(strArg));
                    }
                    // 띄어쓰기가 있는 문자열인 경우 각 단어를 개별적으로 번역
                    else if (strArg.Contains(" "))
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
            
            // 한국어 패턴 적용
            if (args.Length == 3 && args[0] is string && args[2] is string)
            {
                // "Deal 14 Damage" -> "14 데미지로 공격"
                if (args[0].ToString() == "Deal" && args[2].ToString() == "Damage")
                {
                    return $"{args[1]} {TranslateWord("Damage")}로 {TranslateWord("Deal")}";
                }
                // "Gain 5 Block" -> "5 방어도 획득"
                else if (args[0].ToString() == "Gain" && args[2].ToString() == "Block")
                {
                    return $"{args[1]} {TranslateWord("Block")} {TranslateWord("Gain")}";
                }
                // "Draw 2 Cards" -> "카드 2장 뽑기"
                else if (args[0].ToString() == "Draw" && (args[2].ToString() == "Card" || args[2].ToString() == "Cards"))
                {
                    return $"{TranslateWord("Card")} {args[1]}장 {TranslateWord("Draw")}";
                }
            }
            
            // 특수 패턴 처리 - 띄어쓰기가 있는 문구 전체를 키로 사용
            string fullPhrase = string.Join(" ", args);
            string translatedPhrase = TranslateFullPhrase(fullPhrase);
            if (translatedPhrase != fullPhrase)
            {
                return translatedPhrase;
            }
            
            // 기본 패턴: 번역된 인자들을 공백으로 연결
            return string.Join(" ", translatedArgs);
        }
        
        return string.Join(" ", args);
    }

    /// <summary>
    /// 전체 문구를 번역합니다. 사전에 전체 문구가 있으면 해당 번역을 반환합니다.
    /// </summary>
    public string TranslateFullPhrase(string phrase)
    {
        // 전체 문구를 키로 사용하여 번역 시도
        string translatedPhrase = TranslateWord(phrase);
        
        // 번역이 없으면 원본 반환
        if (translatedPhrase == phrase)
        {
            // 숫자 치환 패턴 처리
            return TranslateWithNumberPattern(phrase);
        }
        
        return translatedPhrase;
    }

    /// <summary>
    /// 숫자가 포함된 패턴을 번역합니다.
    /// </summary>
    public string TranslateWithNumberPattern(string phrase)
    {
        // 숫자 추출을 위한 정규식
        var regex = new System.Text.RegularExpressions.Regex(@"\d+");
        var match = regex.Match(phrase);
        
        if (match.Success)
        {
            // 숫자 추출
            string number = match.Value;
            
            // 숫자를 제외한 패턴 생성 (숫자를 {0}으로 치환)
            string pattern = regex.Replace(phrase, "{0}");
            
            // 패턴 번역 시도
            string translatedPattern = TranslateWord(pattern);
            
            // 패턴이 번역되었으면 숫자 삽입
            if (translatedPattern != pattern)
            {
                return string.Format(translatedPattern, number);
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
            if (parts.Length >= 2)
            {
                string key = parts[0].Trim();
                string translation = parts[1].Trim();
                AddWord(key, language, translation);
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
