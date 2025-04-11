using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CCGKit;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace CCGKit
{
    [Serializable]
    public class GeneratedPattern : Pattern
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }

        public override string GetName()
        {
            return Name;
        }

#if UNITY_EDITOR
        public override void Draw()
        {
            // 에디터에서 패턴을 그리는 로직
        }
#endif
    }

    [Serializable]
    public class GeneratedEffect : Effect
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("value")]
        public int Value { get; set; }
        
        [JsonProperty("target")]
        public string Target { get; set; }
        
        [JsonProperty("duration")]
        public int Duration { get; set; }

        public override string GetName()
        {
            return $"{Type} ({Value})";
        }

#if UNITY_EDITOR
        public override void Draw()
        {
            // 에디터에서 효과를 그리는 로직
        }
#endif
    }

    public class PatternGenerator : MonoBehaviour
    {
        private static PatternGenerator instance;
        public static PatternGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("PatternGenerator");
                    instance = go.AddComponent<PatternGenerator>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private Dictionary<string, List<Pattern>> patternCache = new Dictionary<string, List<Pattern>>();
        private DeepInfraAPI deepInfraAPI;
        private const string CACHE_FILE_NAME = "patternCache.json";

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            deepInfraAPI = gameObject.AddComponent<DeepInfraAPI>();
            LoadCachedPatterns();
        }

        public async Task<List<Pattern>> GeneratePatternsAsync(EnemyTemplate enemyTemplate, HeroTemplate playerTemplate, int numPatterns)
        {
            string cacheKey = GenerateCacheKey(enemyTemplate);
            
            if (TryGetCachedPatterns(cacheKey, out var cachedPatterns))
            {
                Debug.Log($"[PatternGenerator] 캐시된 패턴 사용: {cacheKey} (패턴 수: {cachedPatterns.Count})");
                return cachedPatterns;
            }

            Debug.Log($"[PatternGenerator] 새로운 패턴 생성: {cacheKey}");
            var patterns = GeneratePatternsFromLLM(enemyTemplate, playerTemplate, numPatterns);
            CachePatterns(cacheKey, patterns);
            
            return patterns;
        }

        private string GenerateCacheKey(EnemyTemplate enemyTemplate)
        {
            return $"{enemyTemplate.name}_{enemyTemplate.IsBoss}_{enemyTemplate.IsElite}";
        }

        private bool TryGetCachedPatterns(string cacheKey, out List<Pattern> patterns)
        {
            if (patternCache.TryGetValue(cacheKey, out patterns) && 
                patterns != null && 
                patterns.Count > 0)
            {
                return true;
            }
            return false;
        }

        private void CachePatterns(string cacheKey, List<Pattern> patterns)
        {
            patternCache[cacheKey] = patterns;
            SaveCachedPatterns();
        }

        private List<Pattern> GeneratePatternsFromLLM(EnemyTemplate enemyTemplate, HeroTemplate playerTemplate, int numPatterns)
        {
            string response = deepInfraAPI.GeneratePattern(enemyTemplate, playerTemplate, numPatterns);
            if (string.IsNullOrEmpty(response))
            {
                Debug.LogError("[PatternGenerator] LLM에서 패턴 생성 실패");
                return GenerateDefaultPatterns(enemyTemplate, numPatterns);
            }

            try
            {
                return ParsePatternsFromResponse(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PatternGenerator] LLM 응답 파싱 실패: {e.Message}");
                return GenerateDefaultPatterns(enemyTemplate, numPatterns);
            }
        }

        private List<Pattern> ParsePatternsFromResponse(string response)
        {
            try
            {
                if (string.IsNullOrEmpty(response))
                {
                    Debug.LogError("[PatternGenerator] 빈 응답을 받았습니다");
                    return GenerateDefaultPatterns(null, 3);
                }

                Debug.Log($"[PatternGenerator] 파싱할 응답: {response}");
                
                // JSON 응답이 올바른 구조인지 확인
                if (!response.Trim().StartsWith("{") || !response.Trim().EndsWith("}"))
                {
                    Debug.LogError("[PatternGenerator] JSON 응답이 올바른 구조가 아닙니다");
                    return GenerateDefaultPatterns(null, 3);
                }

                var patternData = JsonConvert.DeserializeObject<PatternResponse>(response);
                if (patternData == null)
                {
                    Debug.LogError("[PatternGenerator] JSON 파싱 실패");
                    return GenerateDefaultPatterns(null, 3);
                }

                if (patternData.patterns == null || patternData.patterns.Count == 0)
                {
                    Debug.LogError("[PatternGenerator] 패턴 데이터가 비어있습니다");
                    return GenerateDefaultPatterns(null, 3);
                }

                var patterns = new List<Pattern>();
                foreach (var pattern in patternData.patterns)
                {
                    if (pattern == null)
                    {
                        Debug.LogError("[PatternGenerator] null 패턴 발견");
                        continue;
                    }

                    var generatedPattern = ScriptableObject.CreateInstance<GeneratedPattern>();
                    generatedPattern.Name = pattern.name ?? "이름 없는 패턴";
                    generatedPattern.Description = pattern.description ?? "설명 없는 패턴";
                    generatedPattern.Effects = new List<Effect>();

                    if (pattern.effects != null)
                    {
                        foreach (var effect in pattern.effects)
                        {
                            if (effect == null)
                            {
                                Debug.LogError("[PatternGenerator] null 효과 발견");
                                continue;
                            }

                            var generatedEffect = ScriptableObject.CreateInstance<GeneratedEffect>();
                            generatedEffect.Type = effect.type ?? "Damage";
                            generatedEffect.Value = effect.value;
                            generatedEffect.Target = effect.target ?? "Galahad";
                            generatedEffect.Duration = effect.duration;

                            generatedPattern.Effects.Add(generatedEffect);
                        }
                    }

                    if (generatedPattern.Effects.Count == 0)
                    {
                        Debug.LogWarning("[PatternGenerator] 효과가 없는 패턴에 기본 효과 추가");
                        var defaultEffect = ScriptableObject.CreateInstance<GeneratedEffect>();
                        defaultEffect.Type = "Damage";
                        defaultEffect.Value = 5;
                        defaultEffect.Target = "Galahad";
                        defaultEffect.Duration = 1;
                        generatedPattern.Effects.Add(defaultEffect);
                    }

                    patterns.Add(generatedPattern);
                }

                if (patterns.Count == 0)
                {
                    Debug.LogError("[PatternGenerator] 유효한 패턴이 없습니다");
                    return GenerateDefaultPatterns(null, 3);
                }

                return patterns;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PatternGenerator] 패턴 파싱 실패: {e.Message}");
                Debug.LogError($"[PatternGenerator] 원본 응답: {response}");
                return GenerateDefaultPatterns(null, 3);
            }
        }

        private List<Pattern> GenerateDefaultPatterns(EnemyTemplate enemyTemplate, int numPatterns)
        {
            var patterns = new List<Pattern>();
            for (int i = 0; i < numPatterns; i++)
            {
                var pattern = ScriptableObject.CreateInstance<GeneratedPattern>();
                pattern.Name = $"기본 패턴 {i + 1}";
                pattern.Description = "LLM 실패로 인해 생성된 기본 패턴";
                pattern.Effects = new List<Effect>();

                var effect = ScriptableObject.CreateInstance<GeneratedEffect>();
                effect.Type = "Damage";
                effect.Value = 5;
                effect.Target = "Galahad";
                effect.Duration = 1;
                pattern.Effects.Add(effect);

                patterns.Add(pattern);
            }
            return patterns;
        }

        private void SaveCachedPatterns()
        {
            try
            {
                var cacheData = new PatternCacheData
                {
                    patterns = patternCache.Select(kvp => new PatternCacheEntry
                    {
                        key = kvp.Key,
                        patterns = kvp.Value.Select(p => 
                        {
                            var generatedPattern = p as GeneratedPattern;
                            return new PatternData
                            {
                                name = generatedPattern.Name,
                                description = generatedPattern.Description,
                                effects = generatedPattern.Effects.Select(e => 
                                {
                                    var generatedEffect = e as GeneratedEffect;
                                    return new EffectData
                                    {
                                        type = generatedEffect.Type,
                                        value = generatedEffect.Value,
                                        target = generatedEffect.Target,
                                        duration = generatedEffect.Duration
                                    };
                                }).ToList()
                            };
                        }).ToList()
                    }).ToList()
                };

                string json = JsonConvert.SerializeObject(cacheData, Formatting.Indented);
                string path = Path.Combine(Application.persistentDataPath, CACHE_FILE_NAME);
                File.WriteAllText(path, json);
                
                Debug.Log($"[PatternGenerator] 패턴 캐시 저장 위치: {path}");
                Debug.Log($"[PatternGenerator] 저장된 패턴 수: {patternCache.Values.Sum(patterns => patterns.Count)}");
                Debug.Log($"[PatternGenerator] 캐시된 적 유형 수: {patternCache.Keys.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PatternGenerator] 패턴 캐시 저장 실패: {e.Message}");
            }
        }

        private void LoadCachedPatterns()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, CACHE_FILE_NAME);
                Debug.Log($"[PatternGenerator] 패턴 캐시 파일 위치: {path}");
                
                if (!File.Exists(path))
                {
                    Debug.Log("[PatternGenerator] 캐시 파일이 존재하지 않습니다");
                    return;
                }

                string json = File.ReadAllText(path);
                var cacheData = JsonConvert.DeserializeObject<PatternCacheData>(json);
                if (cacheData == null || cacheData.patterns == null)
                {
                    Debug.LogError("[PatternGenerator] 잘못된 캐시 형식");
                    return;
                }

                patternCache = cacheData.patterns.ToDictionary(
                    entry => entry.key,
                    entry => entry.patterns.Select(p => 
                    {
                        var pattern = ScriptableObject.CreateInstance<GeneratedPattern>();
                        pattern.Name = p.name;
                        pattern.Description = p.description;
                        pattern.Effects = p.effects.Select(e => 
                        {
                            var effect = ScriptableObject.CreateInstance<GeneratedEffect>();
                            effect.Type = e.type;
                            effect.Value = e.value;
                            effect.Target = e.target;
                            effect.Duration = e.duration;
                            return effect;
                        }).Cast<Effect>().ToList();
                        return pattern;
                    }).Cast<Pattern>().ToList()
                );

                Debug.Log($"[PatternGenerator] 캐시된 패턴 로드 완료");
                Debug.Log($"[PatternGenerator] 로드된 패턴 수: {patternCache.Values.Sum(patterns => patterns.Count)}");
                Debug.Log($"[PatternGenerator] 로드된 적 유형 수: {patternCache.Keys.Count}");
                foreach (var key in patternCache.Keys)
                {
                    Debug.Log($"[PatternGenerator] 적 유형: {key}, 패턴 수: {patternCache[key].Count}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[PatternGenerator] 패턴 캐시 로드 실패: {e.Message}");
            }
        }

        [Serializable]
        private class PatternResponse
        {
            [JsonProperty("patterns")]
            public List<PatternData> patterns { get; set; }
        }

        [Serializable]
        private class PatternData
        {
            [JsonProperty("name")]
            public string name { get; set; }
            
            [JsonProperty("description")]
            public string description { get; set; }
            
            [JsonProperty("effects")]
            public List<EffectData> effects { get; set; }
        }

        [Serializable]
        private class EffectData
        {
            [JsonProperty("type")]
            public string type { get; set; }
            
            [JsonProperty("value")]
            public int value { get; set; }
            
            [JsonProperty("target")]
            public string target { get; set; }
            
            [JsonProperty("duration")]
            public int duration { get; set; }
        }

        [Serializable]
        private class PatternCacheData
        {
            public List<PatternCacheEntry> patterns;
        }

        [Serializable]
        private class PatternCacheEntry
        {
            public string key;
            public List<PatternData> patterns;
        }
    }
} 