// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// The "Cards" tab in the editor.
    /// </summary>
    public class CardsTab : EditorTab
    {
        private CardTemplate currentCard;

        private ReorderableList effectsList;
        private Effect currentEffect;
        
        // 새 카드 생성 경로
        private const string CARD_DATA_PATH = "Assets/_Assets/GameData/Cards/";
        
        // 마지막으로 생성된 카드 ID를 저장하는 키
        private const string LAST_CARD_ID_KEY = "CCGKit_LastCardID";

        public CardsTab(SinglePlayerCcgKitEditor editor) :
            base(editor)
        {
        }

        public override void Draw()
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;

            GUILayout.Space(15);
            
            // 새 카드 추가 버튼
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                if (GUILayout.Button("새 카드 추가", GUILayout.Width(120)))
                {
                    CreateNewCard();
                }
                
                // 현재 카드가 선택되어 있을 때만 복제 버튼 활성화
                GUI.enabled = currentCard != null;
                if (GUILayout.Button("카드 복제", GUILayout.Width(120)))
                {
                    DuplicateCard(currentCard);
                }
                
                // 현재 카드가 선택되어 있을 때만 삭제 버튼 활성화
                if (GUILayout.Button("카드 삭제", GUILayout.Width(120)))
                {
                    DeleteCard(currentCard);
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);

            var prevCard = currentCard;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                currentCard = (CardTemplate)EditorGUILayout.ObjectField(
                    "Asset", currentCard, typeof(CardTemplate), false, GUILayout.Width(340));
            }
            GUILayout.EndHorizontal();

            if (currentCard != prevCard)
            {
                if (currentCard != null)
                {
                    CreateEffectsList();
                    currentEffect = null;
                }
            }

            if (currentCard != null)
            {
                DrawCurrentCard();

                if (GUI.changed)
                    EditorUtility.SetDirty(currentCard);
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
        
        /// <summary>
        /// 새 카드를 생성합니다.
        /// </summary>
        private void CreateNewCard()
        {
            // 카드 데이터 경로가 존재하는지 확인하고 없으면 생성
            if (!Directory.Exists(CARD_DATA_PATH))
            {
                Directory.CreateDirectory(CARD_DATA_PATH);
                AssetDatabase.Refresh();
            }
            
            // 마지막으로 생성된 카드 ID 가져오기
            int lastCardId = EditorPrefs.GetInt(LAST_CARD_ID_KEY, 0);
            int newCardId = lastCardId + 1;
            
            // 새 카드 생성
            CardTemplate newCard = ScriptableObject.CreateInstance<CardTemplate>();
            newCard.Id = newCardId;
            newCard.Name = "New Card " + newCardId;
            newCard.Cost = 1;
            
            // 기본 카드 타입 설정 (기존 카드 타입이 있으면 가져오기)
            string[] cardTypeGuids = AssetDatabase.FindAssets("t:CardType");
            if (cardTypeGuids.Length > 0)
            {
                string cardTypePath = AssetDatabase.GUIDToAssetPath(cardTypeGuids[0]);
                CardType defaultCardType = AssetDatabase.LoadAssetAtPath<CardType>(cardTypePath);
                newCard.Type = defaultCardType;
            }
            
            // 기본 카드 재질 설정 (기존 카드 재질이 있으면 가져오기)
            string[] cardMaterialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            foreach (string guid in cardMaterialGuids)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(guid);
                if (materialPath.Contains("Card"))
                {
                    Material cardMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    newCard.Material = cardMaterial;
                    break;
                }
            }
            
            // 카드 에셋 저장 (카드id_카드이름 형식으로 파일명 생성)
            string cardNameForFile = newCard.Name.Replace(" ", "_");
            cardNameForFile = System.Text.RegularExpressions.Regex.Replace(cardNameForFile, @"[^\w\d_]", "");
            string assetPath = CARD_DATA_PATH + newCardId + "_" + cardNameForFile + ".asset";
            AssetDatabase.CreateAsset(newCard, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 마지막 카드 ID 업데이트
            EditorPrefs.SetInt(LAST_CARD_ID_KEY, newCardId);
            
            // 생성된 카드 선택
            currentCard = newCard;
            Selection.activeObject = newCard;
            
            Debug.Log("새 카드가 생성되었습니다: " + assetPath);
            
            // 효과 리스트 생성
            CreateEffectsList();
            
            // Addressables 시스템에 카드 등록
            RegisterCardToAddressables(assetPath);
            
            // 번역 항목 추가
            AddCardTranslation(newCard);
        }

        /// <summary>
        /// 카드를 Addressables 시스템에 등록합니다.
        /// </summary>
        private void RegisterCardToAddressables(string cardAssetPath)
        {
            try
            {
                // Parser_CardList의 SetupCardAddressables 메서드 호출
                // 리플렉션을 사용하여 메서드 호출
                Type parserType = Type.GetType("Parser_CardList");
                if (parserType == null)
                {
                    // 네임스페이스가 없는 경우 모든 어셈블리에서 검색
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        parserType = assembly.GetType("Parser_CardList");
                        if (parserType != null)
                            break;
                    }
                }
                
                if (parserType != null)
                {
                    var setupMethod = parserType.GetMethod("SetupCardAddressables", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    
                    if (setupMethod != null)
                    {
                        setupMethod.Invoke(null, null);
                        Debug.Log("카드가 Addressables 시스템에 등록되었습니다.");
                        
                        // Addressables 빌드 안내
                        if (EditorUtility.DisplayDialog("Addressables 설정 완료", 
                            "새 카드가 Addressables 시스템에 등록되었습니다. Addressables 빌드를 수행하시겠습니까?", 
                            "빌드 수행", "나중에 하기"))
                        {
                            // Addressables 빌드 메뉴 실행
                            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
                            Debug.Log("Addressables 창이 열렸습니다. 'Build > New Build > Default Build Script'를 선택하여 빌드를 수행하세요.");
                        }
                        
                        return;
                    }
                }
                
                // 직접 구현 (Parser_CardList.SetupCardAddressables 메서드를 호출할 수 없는 경우)
                Debug.LogWarning("Parser_CardList.SetupCardAddressables 메서드를 찾을 수 없습니다. 카드를 수동으로 Addressables 시스템에 등록해야 합니다.");
                
                // 사용자에게 안내
                EditorUtility.DisplayDialog("수동 등록 필요", 
                    "카드를 Addressables 시스템에 자동으로 등록할 수 없습니다. 'Tools > Setup Card Addressables' 메뉴를 실행하여 수동으로 등록하세요.", 
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError("카드를 Addressables 시스템에 등록하는 중 오류가 발생했습니다: " + e.Message);
            }
        }

        /// <summary>
        /// 카드에 대한 번역 항목을 추가합니다.
        /// </summary>
        private void AddCardTranslation(CardTemplate card)
        {
            try
            {
                // CardTranslationGenerator 클래스 찾기
                Type translatorType = Type.GetType("CardTranslationGenerator");
                if (translatorType == null)
                {
                    // 네임스페이스가 없는 경우 모든 어셈블리에서 검색
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        translatorType = assembly.GetType("CardTranslationGenerator");
                        if (translatorType != null)
                            break;
                    }
                }
                
                if (translatorType != null)
                {
                    // 번역 파일 경로
                    const string LANGUAGE_FILE_PATH = "Assets/Resources/Languages/korean.txt";
                    
                    // 번역 파일이 존재하는지 확인
                    if (File.Exists(LANGUAGE_FILE_PATH))
                    {
                        // 번역 파일 읽기
                        string[] lines = File.ReadAllLines(LANGUAGE_FILE_PATH);
                        List<string> updatedLines = new List<string>(lines);
                        
                        // 카드 이름 번역 항목 추가
                        string cardNameKey = card.Name;
                        string cardNameKorean = "새 카드 " + card.Id; // 기본 한국어 번역
                        string cardNameEnglish = card.Name; // 기본 영어 번역
                        
                        // 카드 이름 섹션 찾기
                        int cardNameSectionIndex = -1;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].Trim() == "# 카드 이름들")
                            {
                                cardNameSectionIndex = i;
                                break;
                            }
                        }
                        
                        // 카드 이름 섹션이 있으면 그 아래에 추가
                        if (cardNameSectionIndex >= 0)
                        {
                            updatedLines.Insert(cardNameSectionIndex + 1, cardNameKey + "," + cardNameKorean + "," + cardNameEnglish);
                        }
                        else
                        {
                            // 카드 이름 섹션이 없으면 파일 끝에 추가
                            updatedLines.Add("");
                            updatedLines.Add("# 카드 이름들");
                            updatedLines.Add(cardNameKey + "," + cardNameKorean + "," + cardNameEnglish);
                        }
                        
                        // 파일 저장
                        File.WriteAllLines(LANGUAGE_FILE_PATH, updatedLines);
                        AssetDatabase.Refresh();
                        
                        Debug.Log("카드 번역 항목이 추가되었습니다: " + cardNameKey);
                        
                        // 사용자에게 안내
                        EditorUtility.DisplayDialog("번역 항목 추가 완료", 
                            "카드 번역 항목이 추가되었습니다. 필요한 경우 'Tools > Card Translation Generator' 메뉴를 실행하여 번역을 수정하세요.", 
                            "확인");
                    }
                    else
                    {
                        Debug.LogWarning("번역 파일을 찾을 수 없습니다: " + LANGUAGE_FILE_PATH);
                    }
                }
                else
                {
                    Debug.LogWarning("CardTranslationGenerator 클래스를 찾을 수 없습니다. 카드 번역 항목을 수동으로 추가해야 합니다.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("카드 번역 항목을 추가하는 중 오류가 발생했습니다: " + e.Message);
            }
        }

        private void DrawCurrentCard()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(10);

                    GUILayout.BeginVertical("GroupBox", GUILayout.Width(100));
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Id", "The unique identifier of this card."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));
                                currentCard.Id = EditorGUILayout.IntField(currentCard.Id, GUILayout.Width(30));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Name", "The name of this card."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));
                                string oldName = currentCard.Name;
                                currentCard.Name = EditorGUILayout.TextField(currentCard.Name, GUILayout.Width(150));
                                
                                // 이름이 변경되었을 때 스크립터블 오브젝트 이름도 변경
                                if (oldName != currentCard.Name)
                                {
                                    RenameCardAsset(currentCard, oldName);
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Cost", "The cost of this card."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));
                                currentCard.Cost = EditorGUILayout.IntField(currentCard.Cost, GUILayout.Width(30));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Type", "The type of this card."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));
                                currentCard.Type = (CardType)EditorGUILayout.ObjectField(
                                    currentCard.Type, typeof(CardType), false, GUILayout.Width(200));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Material", "The material of this card."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));
                                currentCard.Material = (Material)EditorGUILayout.ObjectField(
                                    "", currentCard.Material, typeof(Material), false, GUILayout.Width(200));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Picture", "The picture of this card."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));
                                currentCard.Picture = (Sprite)EditorGUILayout.ObjectField(
                                    "", currentCard.Picture, typeof(Sprite), false, GUILayout.Width(70));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField(new GUIContent("Upgrade", "The card this one upgrades to."),
                                    GUILayout.Width(EditorGUIUtility.labelWidth));

                                // 업그레이드 카드 표시
                                if (currentCard.Upgrade != null)
                                {
                                    // 단일 객체 처리
                                    currentCard.Upgrade = (CardTemplate)EditorGUILayout.ObjectField(
                                        "", currentCard.Upgrade, typeof(CardTemplate), false, GUILayout.Width(200));
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(10);

                    GUILayout.BeginVertical(GUILayout.Width(300));
                    {
                        effectsList?.DoLayoutList();
                    }
                    GUILayout.EndVertical();

                    if (effectsList != null)
                        DrawCurrentEffect();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawCurrentEffect()
        {
            if (currentEffect != null)
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(17);

                    GUILayout.BeginHorizontal();
                    {
                        currentEffect.CreateSourceActionsList();
                        currentEffect.CreateTargetActionsList();
                        currentEffect.Draw();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private void CreateEffectsList()
        {
            effectsList = SetupReorderableList("Effects", currentCard.Effects, (rect, x) =>
                {
                },
                x =>
                {
                    currentEffect = x;
                },
                () =>
                {
                    var menu = new GenericMenu();
                    menu.AddItem(
                        new GUIContent("Deal damage"), false, CreateEffectCallback, typeof(DealDamageEffect));
                    menu.AddItem(
                        new GUIContent("Gain mana"), false, CreateEffectCallback, typeof(GainManaEffect));
                    menu.AddItem(
                        new GUIContent("Gain HP"), false, CreateEffectCallback, typeof(GainHpEffect));
                    menu.AddItem(
                        new GUIContent("Gain shield"), false, CreateEffectCallback, typeof(GainShieldEffect));
                    menu.AddItem(
                        new GUIContent("Apply buff"), false, CreateEffectCallback, typeof(ApplyStatusEffect));
                    menu.AddItem(
                        new GUIContent("Draw cards"), false, CreateEffectCallback, typeof(DrawCardsEffect));
                    menu.AddItem(
                        new GUIContent("Clone card"), false, CreateEffectCallback, typeof(CloneCardEffect));
                    menu.ShowAsContext();
                },
                x =>
                {
                    UnityEngine.Object.DestroyImmediate(currentEffect, true);
                    currentEffect = null;
                });
                
            effectsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = currentCard.Effects[index];
                
                rect.y += 2;
                rect.width -= 10;
                rect.height = EditorGUIUtility.singleLineHeight;

                var label = element.GetName();
                EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
                rect.y += 5;
                rect.y += EditorGUIUtility.singleLineHeight;

                element.Draw(rect);
            };

            effectsList.elementHeightCallback = (index) =>
            {
                var element = currentCard.Effects[index];
                return element.GetHeight();
            };
        }

        private void CreateEffectCallback(object obj)
        {
            var effect = ScriptableObject.CreateInstance((Type)obj) as Effect;
            if (effect != null)
            {
                effect.hideFlags = HideFlags.HideInHierarchy;
                currentCard.Effects.Add(effect);
                AssetDatabase.AddObjectToAsset(effect, currentCard);
            }
        }

        /// <summary>
        /// 카드 스크립터블 오브젝트의 파일 이름을 변경합니다.
        /// </summary>
        private void RenameCardAsset(CardTemplate card, string oldCardName)
        {
            try
            {
                // 카드 에셋 경로 가져오기
                string assetPath = AssetDatabase.GetAssetPath(card);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning("카드 에셋 경로를 찾을 수 없습니다.");
                    return;
                }
                
                // 새 파일 이름 생성 (공백 제거 및 특수문자 처리)
                string newFileName = card.Name.Replace(" ", "_");
                newFileName = System.Text.RegularExpressions.Regex.Replace(newFileName, @"[^\w\d_]", "");
                
                // 새 경로 생성 (카드id_카드이름 형식)
                string directory = Path.GetDirectoryName(assetPath);
                string extension = Path.GetExtension(assetPath);
                string newPath = Path.Combine(directory, card.Id + "_" + newFileName + extension);
                
                // 경로가 같으면 변경하지 않음
                if (assetPath == newPath)
                    return;
                
                // 이미 같은 이름의 파일이 있는지 확인
                if (File.Exists(newPath))
                {
                    // 타임스탬프 추가
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    newPath = Path.Combine(directory, card.Id + "_" + newFileName + "_" + timestamp + extension);
                }
                
                // 에셋 이름 변경
                AssetDatabase.RenameAsset(assetPath, Path.GetFileNameWithoutExtension(newPath));
                AssetDatabase.SaveAssets();
                
                Debug.Log($"카드 에셋 이름이 변경되었습니다: {Path.GetFileName(assetPath)} -> {Path.GetFileName(newPath)}");
                
                // 번역 파일에서도 이름 업데이트 (카드의 이전 이름과 새 이름 사용)
                UpdateCardNameInTranslationFile(oldCardName, card.Name);
            }
            catch (Exception e)
            {
                Debug.LogError("카드 에셋 이름을 변경하는 중 오류가 발생했습니다: " + e.Message);
            }
        }
        
        /// <summary>
        /// 번역 파일에서 카드 이름을 업데이트합니다.
        /// </summary>
        private void UpdateCardNameInTranslationFile(string oldName, string newName)
        {
            try
            {
                const string LANGUAGE_FILE_PATH = "Assets/Resources/Languages/korean.txt";
                
                // 번역 파일이 존재하는지 확인
                if (File.Exists(LANGUAGE_FILE_PATH))
                {
                    // 번역 파일 읽기
                    string[] lines = File.ReadAllLines(LANGUAGE_FILE_PATH);
                    bool updated = false;
                    
                    // 기존 항목 찾아서 업데이트
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (line.StartsWith(oldName + ","))
                        {
                            string[] parts = line.Split(',');
                            if (parts.Length >= 3)
                            {
                                // 키만 변경하고 번역은 유지
                                lines[i] = newName + "," + parts[1] + "," + parts[2];
                                updated = true;
                                break;
                            }
                        }
                    }
                    
                    // 변경된 내용이 있으면 파일 저장
                    if (updated)
                    {
                        File.WriteAllLines(LANGUAGE_FILE_PATH, lines);
                        AssetDatabase.Refresh();
                        Debug.Log("번역 파일에서 카드 이름이 업데이트되었습니다: " + oldName + " -> " + newName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("번역 파일에서 카드 이름을 업데이트하는 중 오류가 발생했습니다: " + e.Message);
            }
        }

        /// <summary>
        /// 선택된 카드를 복제합니다.
        /// </summary>
        private void DuplicateCard(CardTemplate sourceCard)
        {
            try
            {
                // 카드 데이터 경로가 존재하는지 확인하고 없으면 생성
                if (!Directory.Exists(CARD_DATA_PATH))
                {
                    Directory.CreateDirectory(CARD_DATA_PATH);
                    AssetDatabase.Refresh();
                }
                
                // 마지막으로 생성된 카드 ID 가져오기
                int lastCardId = EditorPrefs.GetInt(LAST_CARD_ID_KEY, 0);
                int newCardId = lastCardId + 1;
                
                // 새 카드 생성 (복제)
                CardTemplate newCard = ScriptableObject.Instantiate(sourceCard);
                newCard.Id = newCardId;
                newCard.Name = sourceCard.Name + " Copy";
                
                // 카드 에셋 저장 (카드id_카드이름 형식으로 파일명 생성)
                string cardNameForFile = newCard.Name.Replace(" ", "_");
                cardNameForFile = System.Text.RegularExpressions.Regex.Replace(cardNameForFile, @"[^\w\d_]", "");
                string assetPath = CARD_DATA_PATH + newCardId + "_" + cardNameForFile + ".asset";
                AssetDatabase.CreateAsset(newCard, assetPath);
                
                // 효과 복제
                newCard.Effects = new List<Effect>(); // 효과 리스트 초기화
                foreach (var effect in sourceCard.Effects)
                {
                    Effect newEffect = ScriptableObject.Instantiate(effect);
                    newEffect.hideFlags = HideFlags.HideInHierarchy;
                    newCard.Effects.Add(newEffect);
                    AssetDatabase.AddObjectToAsset(newEffect, newCard);
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 마지막 카드 ID 업데이트
                EditorPrefs.SetInt(LAST_CARD_ID_KEY, newCardId);
                
                // 생성된 카드 선택
                currentCard = newCard;
                Selection.activeObject = newCard;
                
                Debug.Log("카드가 복제되었습니다: " + assetPath);
                
                // 효과 리스트 생성
                CreateEffectsList();
                
                // Addressables 시스템에 카드 등록
                RegisterCardToAddressables(assetPath);
                
                // 번역 항목 추가
                AddCardTranslation(newCard);
            }
            catch (Exception e)
            {
                Debug.LogError("카드를 복제하는 중 오류가 발생했습니다: " + e.Message);
            }
        }

        /// <summary>
        /// 선택된 카드를 삭제합니다.
        /// </summary>
        private void DeleteCard(CardTemplate card)
        {
            try
            {
                // 확인 대화상자 표시
                if (!EditorUtility.DisplayDialog("카드 삭제 확인", 
                    $"카드 '{card.Name}'을(를) 정말로 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.", 
                    "삭제", "취소"))
                {
                    return;
                }
                
                // 카드 에셋 경로 가져오기
                string assetPath = AssetDatabase.GetAssetPath(card);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning("카드 에셋 경로를 찾을 수 없습니다.");
                    return;
                }
                
                // 번역 파일에서 카드 이름 항목 삭제
                RemoveCardNameFromTranslationFile(card.Name);
                
                // 카드 에셋 삭제
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 현재 카드 선택 해제
                currentCard = null;
                Selection.activeObject = null;
                
                Debug.Log($"카드 '{card.Name}'이(가) 삭제되었습니다.");
                
                // Addressables 시스템 업데이트 안내
                EditorUtility.DisplayDialog("Addressables 업데이트 필요", 
                    "카드가 삭제되었습니다. Addressables 빌드를 수행하여 변경사항을 적용하세요.", 
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError("카드를 삭제하는 중 오류가 발생했습니다: " + e.Message);
            }
        }
        
        /// <summary>
        /// 번역 파일에서 카드 이름 항목을 삭제합니다.
        /// </summary>
        private void RemoveCardNameFromTranslationFile(string cardName)
        {
            try
            {
                const string LANGUAGE_FILE_PATH = "Assets/Resources/Languages/korean.txt";
                
                // 번역 파일이 존재하는지 확인
                if (File.Exists(LANGUAGE_FILE_PATH))
                {
                    // 번역 파일 읽기
                    string[] lines = File.ReadAllLines(LANGUAGE_FILE_PATH);
                    List<string> updatedLines = new List<string>();
                    bool removed = false;
                    
                    // 카드 이름 항목 제외하고 복사
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(cardName + ","))
                        {
                            removed = true;
                            continue;
                        }
                        updatedLines.Add(line);
                    }
                    
                    // 변경된 내용이 있으면 파일 저장
                    if (removed)
                    {
                        File.WriteAllLines(LANGUAGE_FILE_PATH, updatedLines);
                        AssetDatabase.Refresh();
                        Debug.Log("번역 파일에서 카드 이름이 삭제되었습니다: " + cardName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("번역 파일에서 카드 이름을 삭제하는 중 오류가 발생했습니다: " + e.Message);
            }
        }
    }
}
