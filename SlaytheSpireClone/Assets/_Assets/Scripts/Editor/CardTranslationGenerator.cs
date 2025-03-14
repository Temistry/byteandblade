using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using CCGKit;

/// <summary>
/// 카드 데이터와 언어 파일을 자동으로 연동하는 에디터 도구
/// </summary>
public class CardTranslationGenerator : EditorWindow
{
    private const string CARD_DATA_PATH = "Assets/_Assets/GameData/Cards";
    private const string LANGUAGE_FILE_PATH = "Assets/Resources/Languages/korean.txt";
    
    private Dictionary<string, TranslationEntry> translationEntries = new Dictionary<string, TranslationEntry>();
    private List<CardTemplate> cardTemplates = new List<CardTemplate>();
    private Vector2 scrollPosition;
    private bool showMissingTranslations = true;
    private bool showExistingTranslations = true;
    private string koreanTranslation = "";
    private string englishTranslation = "";
    private string searchText = "";
    private bool showPatternTranslations = true;
    
    // 새 번역 항목 추가 관련 변수
    private bool showAddTranslationPanel = false;
    private string newTranslationKey = "";
    private string newTranslationKorean = "";
    private string newTranslationEnglish = "";
    private int selectedTranslationType = 0;
    private string[] translationTypes = new string[] { "기본 단어", "게임 시스템", "카드 관련", "카드 이름", "효과 이름", "패턴 키", "기타" };
    private string newTranslationErrorMessage = "";
    private TranslationEntry selectedEntryForEdit = null;
    private string selectedEntryKey = "";
    private bool confirmDelete = false;
    
    // 번역 항목 클래스
    [Serializable]
    private class TranslationEntry
    {
        public string key;
        public string korean;
        public string english;
        public bool exists;
        public bool isCardName;
        public bool isEffectName;
        public bool isPatternKey;
        
        public TranslationEntry(string key, string korean, string english, bool exists = false)
        {
            this.key = key;
            this.korean = korean;
            this.english = english;
            this.exists = exists;
            this.isCardName = false;
            this.isEffectName = false;
            this.isPatternKey = false;
        }
    }
    
    [MenuItem("Tools/Card Translation Generator")]
    public static void ShowWindow()
    {
        GetWindow<CardTranslationGenerator>("카드 번역 생성기");
    }
    
    private void OnEnable()
    {
        LoadTranslationFile();
        LoadCardTemplates();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("카드 번역 생성기", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // 검색 필드
        searchText = EditorGUILayout.TextField("검색", searchText);
        
        EditorGUILayout.Space();
        
        // 필터 옵션
        EditorGUILayout.BeginHorizontal();
        showMissingTranslations = EditorGUILayout.Toggle("누락된 번역 표시", showMissingTranslations);
        showExistingTranslations = EditorGUILayout.Toggle("기존 번역 표시", showExistingTranslations);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        showPatternTranslations = EditorGUILayout.Toggle("패턴 번역 표시", showPatternTranslations);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 버튼 영역
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("카드 데이터 스캔", GUILayout.Width(150)))
        {
            LoadCardTemplates();
        }
        
        if (GUILayout.Button("언어 파일 로드", GUILayout.Width(150)))
        {
            LoadTranslationFile();
        }
        
        if (GUILayout.Button("누락된 번역 자동 추가", GUILayout.Width(150)))
        {
            AutoFillMissingTranslations();
        }

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("효과 패턴 추출", GUILayout.Width(150)))
        {
            ExtractEffectPatterns();
        }
        
        if (GUILayout.Button("패턴 번역 테스트", GUILayout.Width(150)))
        {
            TestPatternTranslation();
        }

        // 새 번역 추가 버튼
        if (GUILayout.Button("새 번역 추가", GUILayout.Width(150)))
        {
            showAddTranslationPanel = !showAddTranslationPanel;
            newTranslationKey = "";
            newTranslationKorean = "";
            newTranslationEnglish = "";
            newTranslationErrorMessage = "";
            selectedEntryForEdit = null;
        }


        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("언어 파일 저장", GUILayout.Width(350), GUILayout.Height(50)))
        {
            SaveTranslationFile();
        }

        EditorGUILayout.Space();
        
        // 새 번역 추가 패널
        if (showAddTranslationPanel)
        {
            DrawAddTranslationPanel();
        }
        
        EditorGUILayout.Space();
        
        // 선택된 항목 편집 패널
        if (selectedEntryForEdit != null)
        {
            DrawEditTranslationPanel();
        }
        
        EditorGUILayout.Space();
        
        // 번역 항목 표시
        DrawTranslationEntries();
    }
    
    private void DrawAddTranslationPanel()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("새 번역 추가", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // 분류 선택
        selectedTranslationType = EditorGUILayout.Popup("분류", selectedTranslationType, translationTypes);
        
        // 키 필드
        newTranslationKey = EditorGUILayout.TextField("키", newTranslationKey);
        
        // 한국어 번역 필드
        newTranslationKorean = EditorGUILayout.TextField("한국어", newTranslationKorean);
        
        // 영어 번역 필드
        newTranslationEnglish = EditorGUILayout.TextField("영어", newTranslationEnglish);
        
        // 에러 메시지 표시
        if (!string.IsNullOrEmpty(newTranslationErrorMessage))
        {
            EditorGUILayout.HelpBox(newTranslationErrorMessage, MessageType.Error);
        }
        
        EditorGUILayout.Space();
        
        // 버튼 영역
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("추가", GUILayout.Width(100)))
        {
            AddNewTranslation();
        }
        
        if (GUILayout.Button("취소", GUILayout.Width(100)))
        {
            showAddTranslationPanel = false;
            newTranslationErrorMessage = "";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawEditTranslationPanel()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("번역 편집: " + selectedEntryForEdit.key, EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // 한국어 번역 필드
        EditorGUI.BeginChangeCheck();
        selectedEntryForEdit.korean = EditorGUILayout.TextField("한국어", selectedEntryForEdit.korean);
        if (EditorGUI.EndChangeCheck())
        {
            selectedEntryForEdit.exists = true;
        }
        
        // 영어 번역 필드
        EditorGUI.BeginChangeCheck();
        selectedEntryForEdit.english = EditorGUILayout.TextField("영어", selectedEntryForEdit.english);
        if (EditorGUI.EndChangeCheck())
        {
            selectedEntryForEdit.exists = true;
        }
        
        EditorGUILayout.Space();
        
        // 버튼 영역
        EditorGUILayout.BeginHorizontal();
        
        // 삭제 확인
        if (confirmDelete)
        {
            GUILayout.Label("정말 삭제하시겠습니까?", EditorStyles.boldLabel);
            
            if (GUILayout.Button("예", GUILayout.Width(50)))
            {
                DeleteTranslation(selectedEntryForEdit.key);
                selectedEntryForEdit = null;
                confirmDelete = false;
            }
            
            if (GUILayout.Button("아니오", GUILayout.Width(50)))
            {
                confirmDelete = false;
            }
        }
        else
        {
            if (GUILayout.Button("적용", GUILayout.Width(100)))
            {
                selectedEntryForEdit = null;
            }
            
            if (GUILayout.Button("삭제", GUILayout.Width(100)))
            {
                confirmDelete = true;
            }
            
            if (GUILayout.Button("취소", GUILayout.Width(100)))
            {
                selectedEntryForEdit = null;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTranslationEntries()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 카드 이름 섹션
        GUILayout.Label("카드 이름", EditorStyles.boldLabel);
        DrawTranslationSection(entry => entry.isCardName);
        
        EditorGUILayout.Space();
        
        // 효과 이름 섹션
        GUILayout.Label("효과 이름", EditorStyles.boldLabel);
        DrawTranslationSection(entry => entry.isEffectName);
        
        EditorGUILayout.Space();
        
        // 패턴 번역 섹션
        if (showPatternTranslations)
        {
            GUILayout.Label("패턴 번역", EditorStyles.boldLabel);
            DrawTranslationSection(entry => entry.isPatternKey);
            
            EditorGUILayout.Space();
        }
        
        // 기본 단어 섹션
        GUILayout.Label("기본 단어", EditorStyles.boldLabel);
        DrawTranslationSection(entry => !entry.isCardName && !entry.isEffectName && !entry.isPatternKey && IsBasicWord(entry.key));
        
        EditorGUILayout.Space();
        
        // 게임 시스템 관련 섹션
        GUILayout.Label("게임 시스템 관련", EditorStyles.boldLabel);
        DrawTranslationSection(entry => !entry.isCardName && !entry.isEffectName && !entry.isPatternKey && IsSystemRelated(entry.key));
        
        EditorGUILayout.Space();
        
        // 카드 관련 섹션
        GUILayout.Label("카드 관련", EditorStyles.boldLabel);
        DrawTranslationSection(entry => !entry.isCardName && !entry.isEffectName && !entry.isPatternKey && IsCardRelated(entry.key));
        
        EditorGUILayout.Space();
        
        // 기타 번역 섹션
        GUILayout.Label("기타 번역", EditorStyles.boldLabel);
        DrawTranslationSection(entry => !entry.isCardName && !entry.isEffectName && !entry.isPatternKey && 
                              !IsBasicWord(entry.key) && !IsSystemRelated(entry.key) && !IsCardRelated(entry.key));
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawTranslationSection(Func<TranslationEntry, bool> filter)
    {
        var filteredEntries = translationEntries.Values
            .Where(filter)
            .Where(entry => 
                (showMissingTranslations && !entry.exists) || 
                (showExistingTranslations && entry.exists))
            .Where(entry => 
                string.IsNullOrEmpty(searchText) || 
                entry.key.ToLower().Contains(searchText.ToLower()) ||
                entry.korean.ToLower().Contains(searchText.ToLower()) ||
                entry.english.ToLower().Contains(searchText.ToLower()))
            .OrderBy(entry => entry.key)
            .ToList();
        
        if (filteredEntries.Count == 0)
        {
            EditorGUILayout.LabelField("항목 없음");
            return;
        }
        
        foreach (var entry in filteredEntries)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 상태 표시
            if (!entry.exists)
            {
                EditorGUILayout.LabelField("*", GUILayout.Width(10));
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(10));
            }
            
            // 키 (클릭 가능)
            if (GUILayout.Button(entry.key, EditorStyles.label, GUILayout.Width(150)))
            {
                // 항목 선택 (편집 모드)
                if (selectedEntryForEdit == entry)
                {
                    selectedEntryForEdit = null;
                }
                else
                {
                    selectedEntryForEdit = entry;
                    selectedEntryKey = entry.key;
                    confirmDelete = false;
                }
            }
            
            // 한국어 번역
            EditorGUI.BeginChangeCheck();
            entry.korean = EditorGUILayout.TextField(entry.korean, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck())
            {
                entry.exists = true;
            }
            
            // 영어 번역
            EditorGUI.BeginChangeCheck();
            entry.english = EditorGUILayout.TextField(entry.english, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck())
            {
                entry.exists = true;
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
    
    private void LoadCardTemplates()
    {
        cardTemplates.Clear();
        
        // 카드 에셋 로드
        string[] cardGuids = AssetDatabase.FindAssets("t:CardTemplate", new[] { CARD_DATA_PATH });
        foreach (string guid in cardGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CardTemplate cardTemplate = AssetDatabase.LoadAssetAtPath<CardTemplate>(assetPath);
            if (cardTemplate != null)
            {
                cardTemplates.Add(cardTemplate);
                Debug.Log($"카드 로드: {cardTemplate.Name} (ID: {cardTemplate.Id})");
            }
        }
        
        // 카드 이름 추가
        foreach (var card in cardTemplates)
        {
            AddTranslationEntry(card.Name, card.Name, card.Name, true, true, false, false);
            
            // 효과 이름 추가
            foreach (var effect in card.Effects)
            {
                string effectName = effect.GetName();
                AddTranslationEntry(effectName, effectName, effectName, false, false, true, false);
            }
        }
        
        Debug.Log($"총 {cardTemplates.Count}개의 카드를 로드했습니다.");
    }
    
    private void LoadTranslationFile()
    {
        if (!File.Exists(LANGUAGE_FILE_PATH))
        {
            Debug.LogWarning($"언어 파일을 찾을 수 없습니다: {LANGUAGE_FILE_PATH}");
            return;
        }
        
        string[] lines = File.ReadAllLines(LANGUAGE_FILE_PATH);
        translationEntries.Clear();
        
        foreach (string line in lines)
        {
            // 주석 라인 무시
            if (line.StartsWith("#") || line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                continue;
            
            string[] parts = line.Split(',');
            if (parts.Length >= 3)
            {
                string key = parts[0].Trim();
                string korean = parts[1].Trim();
                string english = parts[2].Trim();
                
                // 패턴 키인지 확인
                bool isPatternKey = key.Contains("{0}");
                
                AddTranslationEntry(key, korean, english, true, false, false, isPatternKey);
            }
        }
        
        Debug.Log($"언어 파일에서 {translationEntries.Count}개의 번역 항목을 로드했습니다.");
    }
    
    private void SaveTranslationFile()
    {
        StringBuilder sb = new StringBuilder();
        
        // 헤더 추가
        sb.AppendLine("# 주석은 #으로 시작");
        sb.AppendLine("// 또는 //로 시작하는 주석도 가능");
        sb.AppendLine("# 키값,한국어,영어,기타다른언어등등....");
        
        // 기본 단어 섹션
        sb.AppendLine("");
        sb.AppendLine("# 기본 단어");
        var basicWords = translationEntries.Values
            .Where(e => !e.isCardName && !e.isEffectName && !e.isPatternKey && IsBasicWord(e.key))
            .OrderBy(e => e.key);
        foreach (var entry in basicWords)
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 게임 시스템 관련 섹션
        sb.AppendLine("");
        sb.AppendLine("# 게임 시스템 관련");
        var systemEntries = translationEntries.Values
            .Where(e => !e.isCardName && !e.isEffectName && !e.isPatternKey && IsSystemRelated(e.key))
            .OrderBy(e => e.key);
        foreach (var entry in systemEntries)
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 카드 관련 섹션
        sb.AppendLine("");
        sb.AppendLine("# 카드 관련");
        var cardRelatedEntries = translationEntries.Values
            .Where(e => !e.isCardName && !e.isEffectName && !e.isPatternKey && IsCardRelated(e.key))
            .OrderBy(e => e.key);
        foreach (var entry in cardRelatedEntries)
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 카드 이름 섹션
        sb.AppendLine("");
        sb.AppendLine("# 카드 이름들");
        foreach (var entry in translationEntries.Values.Where(e => e.isCardName).OrderBy(e => e.key))
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 효과 이름 섹션
        sb.AppendLine("");
        sb.AppendLine("# 효과 이름들");
        foreach (var entry in translationEntries.Values.Where(e => e.isEffectName).OrderBy(e => e.key))
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 패턴 번역 섹션
        sb.AppendLine("");
        sb.AppendLine("# 패턴 번역");
        foreach (var entry in translationEntries.Values.Where(e => e.isPatternKey).OrderBy(e => e.key))
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 기타 번역 섹션
        sb.AppendLine("");
        sb.AppendLine("# 기타 번역");
        var otherEntries = translationEntries.Values
            .Where(e => !e.isCardName && !e.isEffectName && !e.isPatternKey && 
                       !IsBasicWord(e.key) && !IsSystemRelated(e.key) && !IsCardRelated(e.key))
            .OrderBy(e => e.key);
        foreach (var entry in otherEntries)
        {
            sb.AppendLine($"{entry.key},{entry.korean},{entry.english}");
        }
        
        // 파일 저장
        File.WriteAllText(LANGUAGE_FILE_PATH, sb.ToString());
        AssetDatabase.Refresh();
        
        Debug.Log($"언어 파일을 저장했습니다: {LANGUAGE_FILE_PATH}");
    }
    
    private bool IsBasicWord(string key)
    {
        string[] basicWords = new string[] 
        { 
            "Deal", "Damage", "Gain", "Block", "Draw", "Card", "Cards", "Reset", 
            "Yes", "No", "Cancel", "OK", "Back", "Exit", "Save", "Load", "Option", 
            "Help", "About", "Language", "English", "Korean", "Player", "Data" 
        };
        
        return basicWords.Contains(key);
    }
    
    private bool IsSystemRelated(string key)
    {
        string[] systemWords = new string[] 
        { 
            "Victory", "Defeat", "Player Turn", "Enemy Turn", "Continue", "New Game", 
            "Activate", "Disable", "Read", "Unread", "No upgradeable cards", 
            "Merge description", "Dungeon defeat", "Boss victory", "Boss victory description", 
            "Exit game?", "reset player data?", "HP", "Gold", "Health", "Loading" 
        };
        
        return systemWords.Contains(key);
    }
    
    private bool IsCardRelated(string key)
    {
        string[] cardWords = new string[] { "Spell" };
        return cardWords.Contains(key);
    }
    
    private void AddTranslationEntry(string key, string korean, string english, bool exists, bool isCardName, bool isEffectName, bool isPatternKey)
    {
        if (string.IsNullOrEmpty(key))
            return;
        
        if (translationEntries.TryGetValue(key, out TranslationEntry existingEntry))
        {
            // 이미 존재하는 항목이면 업데이트하지 않음
            if (isCardName)
            {
                existingEntry.isCardName = true;
            }
            
            if (isEffectName)
            {
                existingEntry.isEffectName = true;
            }
            
            if (isPatternKey)
            {
                existingEntry.isPatternKey = true;
            }
            
            // 존재 여부 업데이트
            if (exists)
            {
                existingEntry.exists = true;
            }
        }
        else
        {
            // 새 항목 추가
            TranslationEntry newEntry = new TranslationEntry(key, korean, english, exists);
            
            newEntry.isCardName = isCardName;
            newEntry.isEffectName = isEffectName;
            newEntry.isPatternKey = isPatternKey;
            
            translationEntries.Add(key, newEntry);
        }
    }
    
    private bool IsEffectName(string key)
    {
        // 효과 이름 패턴 확인 (예: "Deal damage", "Gain shield" 등)
        string pattern = @"^(Deal|Gain|Apply|Draw|Clone|Heal)\s+.*$";
        return Regex.IsMatch(key, pattern);
    }
    
    private void AutoFillMissingTranslations()
    {
        int count = 0;
        
        // 카드 이름 자동 번역
        foreach (var entry in translationEntries.Values.Where(e => e.isCardName && !e.exists))
        {
            // 기본 번역 규칙 적용
            entry.korean = AutoTranslateCardName(entry.key);
            entry.english = entry.key;
            entry.exists = true;
            count++;
        }
        
        // 효과 이름 자동 번역
        foreach (var entry in translationEntries.Values.Where(e => e.isEffectName && !e.exists))
        {
            // 기본 번역 규칙 적용
            entry.korean = AutoTranslateEffectName(entry.key);
            entry.english = entry.key;
            entry.exists = true;
            count++;
        }
        
        // 패턴 키 자동 번역
        foreach (var entry in translationEntries.Values.Where(e => e.isPatternKey && !e.exists))
        {
            // 패턴 번역 규칙 적용
            string testPattern = entry.key.Replace("{0}", "5");
            entry.korean = CardEffectPatternTranslator.TranslateEffectToKorean(testPattern).Replace("5", "{0}");
            entry.english = CardEffectPatternTranslator.TranslateEffectToEnglish(testPattern).Replace("5", "{0}");
            entry.exists = true;
            count++;
        }
        
        Debug.Log($"{count}개의 누락된 번역을 자동으로 추가했습니다.");
    }
    
    private string AutoTranslateCardName(string cardName)
    {
        // 카드 이름 번역 규칙
        Dictionary<string, string> cardNameTranslations = new Dictionary<string, string>
        {
            { "Sword", "검" },
            { "Shield", "방패" },
            { "Potion", "물약" },
            { "Anger", "분노" },
            { "Critical", "치명적인" },
            { "Attack", "공격" },
            { "Magic", "마법" },
            { "Book", "책" },
            { "Fortitude", "인내" },
            { "Inspiration", "영감" },
            { "Toxic", "독성" },
            { "Fog", "안개" },
            { "Double", "이중" },
            { "Triple", "삼중" },
            { "Intense", "강렬한" },
            { "Offering", "헌신" }
        };
        
        // 복합 이름 처리 (예: Sword_x2 -> 이중 검)
        if (cardName.Contains("_x2"))
        {
            string baseName = cardName.Replace("_x2", "");
            if (cardNameTranslations.TryGetValue(baseName, out string baseTranslation))
            {
                return "이중 " + baseTranslation;
            }
        }
        else if (cardName.Contains("_x3"))
        {
            string baseName = cardName.Replace("_x3", "");
            if (cardNameTranslations.TryGetValue(baseName, out string baseTranslation))
            {
                return "삼중 " + baseTranslation;
            }
        }
        
        // 단일 단어 처리
        if (cardNameTranslations.TryGetValue(cardName, out string translation))
        {
            return translation;
        }
        
        // 복합 단어 처리 (예: CriticalAttack -> 치명적인 공격)
        foreach (var pair in cardNameTranslations)
        {
            if (cardName.Contains(pair.Key))
            {
                cardName = cardName.Replace(pair.Key, pair.Value);
            }
        }
        
        return cardName;
    }
    
    private string AutoTranslateEffectName(string effectName)
    {
        // 효과 이름 번역 규칙
        Dictionary<string, string> effectTranslations = new Dictionary<string, string>
        {
            { "Deal damage", "데미지 주기" },
            { "Gain shield", "방어도 획득" },
            { "Gain HP", "체력 회복" },
            { "Gain mana", "마나 획득" },
            { "Apply buff", "버프 적용" },
            { "Draw cards", "카드 뽑기" },
            { "Clone card", "카드 복제" }
        };
        
        if (effectTranslations.TryGetValue(effectName, out string translation))
        {
            return translation;
        }
        
        return effectName;
    }
    
    private void ExtractEffectPatterns()
    {
        HashSet<string> patternKeys = new HashSet<string>();
        
        // 카드 효과에서 패턴 추출
        foreach (var card in cardTemplates)
        {
            foreach (var effect in card.Effects)
            {
                string effectName = effect.GetName();
                string patternKey = CardEffectPatternTranslator.ExtractPatternKey(effectName);
                
                if (patternKey != effectName && patternKey.Contains("{0}"))
                {
                    patternKeys.Add(patternKey);
                }
            }
        }
        
        // 패턴 키 추가
        int count = 0;
        foreach (var patternKey in patternKeys)
        {
            if (!translationEntries.ContainsKey(patternKey))
            {
                // 테스트 패턴으로 번역 생성
                string testPattern = patternKey.Replace("{0}", "5");
                string koreanTranslation = CardEffectPatternTranslator.TranslateEffectToKorean(testPattern).Replace("5", "{0}");
                string englishTranslation = CardEffectPatternTranslator.TranslateEffectToEnglish(testPattern).Replace("5", "{0}");
                
                AddTranslationEntry(patternKey, koreanTranslation, englishTranslation, false, false, false, true);
                count++;
            }
        }
        
        Debug.Log($"{count}개의 새로운 패턴을 추출했습니다.");
    }
    
    private void TestPatternTranslation()
    {
        // 테스트할 패턴
        string[] testPatterns = new string[]
        {
            "Deal 5 damage",
            "Gain 10 Shield",
            "Draw 3 Cards",
            "Draw 1 Card",
            "Gain 5 HP",
            "Apply 2 Poison",
            "Apply 1 Weak",
            "Apply 3 Vulnerable",
            "Apply 2 Strength",
            "Gain 2 Mana",
            "Clone 2 card"
        };
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("패턴 번역 테스트 결과:");
        
        foreach (var pattern in testPatterns)
        {
            string koreanTranslation = CardEffectPatternTranslator.TranslateEffectToKorean(pattern);
            string englishTranslation = CardEffectPatternTranslator.TranslateEffectToEnglish(pattern);
            string patternKey = CardEffectPatternTranslator.ExtractPatternKey(pattern);
            
            sb.AppendLine($"원본: {pattern}");
            sb.AppendLine($"한국어: {koreanTranslation}");
            sb.AppendLine($"영어: {englishTranslation}");
            sb.AppendLine($"패턴 키: {patternKey}");
            sb.AppendLine();
        }
        
        Debug.Log(sb.ToString());
    }
    
    /// <summary>
    /// 새로운 번역 항목을 추가합니다.
    /// </summary>
    private void AddNewTranslation()
    {
        // 유효성 검사
        if (string.IsNullOrEmpty(newTranslationKey))
        {
            newTranslationErrorMessage = "키를 입력해주세요.";
            return;
        }
        
        if (string.IsNullOrEmpty(newTranslationKorean))
        {
            newTranslationErrorMessage = "한국어 번역을 입력해주세요.";
            return;
        }
        
        if (string.IsNullOrEmpty(newTranslationEnglish))
        {
            newTranslationErrorMessage = "영어 번역을 입력해주세요.";
            return;
        }
        
        // 이미 존재하는 키인지 확인
        if (translationEntries.ContainsKey(newTranslationKey) && !confirmDelete)
        {
            newTranslationErrorMessage = "이미 존재하는 키입니다. 기존 항목을 수정하거나 다른 키를 사용해주세요.";
            return;
        }
        
        // 항목 유형 확인
        bool isCardName = false;
        bool isEffectName = false;
        bool isPatternKey = false;
        
        switch (selectedTranslationType)
        {
            case 3: // 카드 이름
                isCardName = true;
                break;
            case 4: // 효과 이름
                isEffectName = true;
                break;
            case 5: // 패턴 키
                isPatternKey = true;
                break;
        }
        
        // 패턴 키 검증
        if (isPatternKey && !newTranslationKey.Contains("{0}"))
        {
            newTranslationErrorMessage = "패턴 키는 {0} 형식의 플레이스홀더를 포함해야 합니다.";
            return;
        }
        
        // 번역 항목 추가
        AddTranslationEntry(newTranslationKey, newTranslationKorean, newTranslationEnglish, true, isCardName, isEffectName, isPatternKey);
        
        // 추가 완료 후 초기화
        newTranslationKey = "";
        newTranslationKorean = "";
        newTranslationEnglish = "";
        newTranslationErrorMessage = "";
        showAddTranslationPanel = false;
        
        Debug.Log($"새 번역 항목이 추가되었습니다.");
    }
    
    /// <summary>
    /// 번역 항목을 삭제합니다.
    /// </summary>
    private void DeleteTranslation(string key)
    {
        if (translationEntries.ContainsKey(key))
        {
            translationEntries.Remove(key);
            Debug.Log($"번역 항목이 삭제되었습니다: {key}");
        }
    }
} 