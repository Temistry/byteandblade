using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using CCGKit;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

public class Parser_EffectSound : Singleton<Parser_EffectSound>
{
    private Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();
    
    // CardSoundData 스크립터블 오브젝트 참조
    [SerializeField] private CardSoundData cardSoundData;
    
    // Addressables 라벨 (효과음 에셋에 이 라벨을 추가해야 함)
    private const string SOUND_EFFECT_ASSETS_LABEL = "SoundEffect";
    // 효과음 데이터 경로
    private const string SOUND_EFFECT_PATH = "Assets/_Assets/effectsound";

    // 로드 상태 추적
    private bool isLoading = false;
    private bool loadFailed = false;
    
    // 로드 완료 이벤트
    public delegate void SoundEffectsLoadedHandler(bool success, int soundCount, string errorMessage);
    public event SoundEffectsLoadedHandler OnSoundEffectsLoaded;

    // 카드 효과 타입 열거형 정의
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
        Critical,
        None
    }

    public override void Awake()
    {
        base.Awake();
        LoadAllSoundEffectsAsync();
    }

    /// <summary>
    /// 이름으로 효과음을 찾습니다.
    /// </summary>
    public AudioClip GetAudioClip(string name)
    {
        if (soundEffects.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }
        
        Debug.LogWarning($"효과음 '{name}'을 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 모든 효과음을 재생 준비 상태로 로드합니다.
    /// </summary>
    public async void LoadAllSoundEffectsAsync()
    {
        // 이미 로드 중이면 중복 로드 방지
        if (isLoading)
        {
            Debug.LogWarning("효과음 데이터가 이미 로드 중입니다.");
            return;
        }

        isLoading = true;
        loadFailed = false;
        soundEffects.Clear();
        
        AsyncOperationHandle initOperation = default;
        AsyncOperationHandle<IList<IResourceLocation>> checkLabelOperation = default;
        AsyncOperationHandle<IList<AudioClip>> loadOperation = default;
        
        try
        {
            // Addressables 시스템 초기화
            Debug.Log("효과음 Addressables 시스템 초기화 중...");

            try
            {
                // 초기화 전에 모든 이전 작업 해제 시도
                await Resources.UnloadUnusedAssets();
                System.GC.Collect();
                
                // 초기화 시도
                initOperation = Addressables.InitializeAsync();
                
                // 동기적으로 완료 대기 (비동기 대신)
                initOperation.WaitForCompletion();
                
                // 유효성 검사
                if (!initOperation.IsValid())
                {
                    throw new System.Exception("Addressables 초기화 핸들이 유효하지 않습니다.");
                }
                
                Debug.Log("Addressables 시스템 초기화 완료");
            }
            catch (System.Exception e)
            {
                throw new System.Exception($"Addressables 초기화 중 오류: {e.Message}");
            }
            
            // 라벨이 존재하는지 확인
            Debug.Log($"'{SOUND_EFFECT_ASSETS_LABEL}' 라벨 확인 중...");
            checkLabelOperation = Addressables.LoadResourceLocationsAsync(SOUND_EFFECT_ASSETS_LABEL);
            await checkLabelOperation.Task;
            
            if (checkLabelOperation.Status == AsyncOperationStatus.Failed)
            {
                throw new System.Exception("라벨 확인 실패: " + checkLabelOperation.OperationException?.Message);
            }
            
            IList<IResourceLocation> locations = checkLabelOperation.Result;
            if (locations == null || locations.Count == 0)
            {
                throw new System.Exception($"'{SOUND_EFFECT_ASSETS_LABEL}' 라벨을 가진 에셋이 없습니다. 'Tools > Setup Sound Effect Addressables' 메뉴를 실행하여 효과음 에셋에 라벨을 추가하세요.");
            }
            
            Debug.Log($"'{SOUND_EFFECT_ASSETS_LABEL}' 라벨을 가진 에셋 {locations.Count}개를 찾았습니다. 로드 시작...");
            
            // 라벨로 모든 효과음 에셋 로드
            loadOperation = Addressables.LoadAssetsAsync<AudioClip>(
                SOUND_EFFECT_ASSETS_LABEL, 
                null);
            
            // 로드 완료 대기
            await loadOperation.Task;
            
            if (loadOperation.Status == AsyncOperationStatus.Failed)
            {
                throw new System.Exception("효과음 에셋 로드 실패: " + loadOperation.OperationException?.Message);
            }
            
            // 결과를 soundEffects에 추가
            var loadedSounds = loadOperation.Result;
            if (loadedSounds != null)
            {
                foreach (var clip in loadedSounds)
                {
                    if (clip != null)
                    {
                        soundEffects[clip.name] = clip;
                        Debug.Log($"효과음 로드: {clip.name}");
                    }
                }
            }
            
            if (soundEffects.Count == 0)
            {
                throw new System.Exception("효과음이 로드되지 않았습니다. 효과음 에셋이 올바르게 설정되었는지 확인하세요.");
            }
            
            Debug.Log($"총 {soundEffects.Count}개의 효과음을 로드했습니다.");
            
            // 로드 완료 이벤트 발생
            OnSoundEffectsLoaded?.Invoke(true, soundEffects.Count, null);
        }
        catch (System.Exception e)
        {
            loadFailed = true;
            string errorMessage = $"효과음 데이터 로드 중 오류 발생: {e.Message}";
            Debug.LogError(errorMessage);
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
            
            // 오류 유형에 따른 상세 안내
            if (e.Message.Contains("Addressables 시스템 초기화 실패"))
            {
                Debug.LogError("해결 방법: Addressables 패키지가 올바르게 설치되었는지 확인하세요. Window > Package Manager에서 Addressables 패키지를 확인하고, 필요한 경우 재설치하세요.");
            }
            else if (e.Message.Contains("라벨 확인 실패"))
            {
                Debug.LogError("해결 방법: Addressables 설정이 올바른지 확인하세요. Window > Asset Management > Addressables > Groups 메뉴에서 설정을 확인하세요.");
            }
            else if (e.Message.Contains("라벨을 가진 에셋이 없습니다"))
            {
                Debug.LogError("해결 방법: 'Tools > Setup Sound Effect Addressables' 메뉴를 실행하여 효과음 에셋에 라벨을 추가하세요. 그 후 Window > Asset Management > Addressables > Groups 메뉴에서 Build > New Build > Default Build Script를 실행하세요.");
            }
            else if (e.Message.Contains("효과음 에셋 로드 실패"))
            {
                Debug.LogError("해결 방법: 효과음 에셋이 유효한지 확인하세요. 손상된 에셋이 있을 수 있습니다. 또한 Addressables 빌드가 최신 상태인지 확인하세요.");
            }
            else if (e.Message.Contains("효과음이 로드되지 않았습니다"))
            {
                Debug.LogError("해결 방법: 효과음 에셋이 올바른 형식(AudioClip)인지 확인하고, 'Tools > Setup Sound Effect Addressables' 메뉴를 다시 실행한 후 Addressables 빌드를 수행하세요.");
            }
            else if (e.Message.Contains("invalid operation handle"))
            {
                Debug.LogError("해결 방법: Addressables 빌드를 다시 수행하세요. Window > Asset Management > Addressables > Groups 메뉴에서 Build > Clean > All을 선택한 후, Build > New Build > Default Build Script를 실행하세요.");
            }
            
            // 로드 실패 이벤트 발생
            OnSoundEffectsLoaded?.Invoke(false, 0, errorMessage);
        }
        finally
        {
            // 핸들 해제
            if (initOperation.IsValid())
            {
                Addressables.Release(initOperation);
            }
            
            if (checkLabelOperation.IsValid())
            {
                Addressables.Release(checkLabelOperation);
            }
            
            // 로드 작업 핸들은 효과음 데이터가 필요한 동안 유지해야 할 수 있으므로,
            // 실패한 경우에만 해제
            if (loadFailed && loadOperation.IsValid())
            {
                Addressables.Release(loadOperation);
            }
            
            isLoading = false;
        }
    }

    /// <summary>
    /// 효과음 로드 상태를 확인합니다.
    /// </summary>
    public bool IsSoundEffectLoadingComplete()
    {
        return !isLoading;
    }
    
    /// <summary>
    /// 효과음 로드가 실패했는지 확인합니다.
    /// </summary>
    public bool HasSoundEffectLoadingFailed()
    {
        return loadFailed;
    }
    
    /// <summary>
    /// 효과음 로드를 다시 시도합니다.
    /// </summary>
    public void RetrySoundEffectLoading()
    {
        if (!isLoading)
        {
            LoadAllSoundEffectsAsync();
        }
    }

    /// <summary>
    /// 이펙트 사운드를 재생합니다.
    /// </summary>
    public void PlaySoundEffect(string soundName, float volume = 1.0f)
    {
        AudioClip clip = GetAudioClip(soundName);
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }
    }

    /// <summary>
    /// AudioSource에 이펙트 사운드를 설정합니다.
    /// </summary>
    public void SetAudioSourceClip(AudioSource audioSource, string soundName)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource가 null입니다.");
            return;
        }

        AudioClip clip = GetAudioClip(soundName);
        if (clip != null)
        {
            audioSource.clip = clip;
        }
    }
    
    /// <summary>
    /// 모든 등록된 사운드 이름 목록을 반환합니다.
    /// </summary>
    public List<string> GetAllSoundNames()
    {
        return soundEffects.Keys.ToList();
    }
    
    /// <summary>
    /// 공격력에 따른 효과음을 재생합니다. CardSoundData가 있으면 CardSoundData의 설정을 사용합니다.
    /// </summary>
    public void PlayAttackPowerSound(int attackPower, float volume = 1.0f)
    {
        string soundName;
        
        // CardSoundData가 있으면 CardSoundData의 설정 사용
        if (cardSoundData != null)
        {
            soundName = cardSoundData.GetSoundForAttackPower(attackPower, true);
            
            // CardSoundData에 설정된 사운드가 있고, 해당 사운드가 로드되어 있으면 사용
            if (!string.IsNullOrEmpty(soundName) && GetAudioClip(soundName) != null)
            {
                PlaySoundEffect(soundName, volume);
                return;
            }
        }
        
        // CardSoundData가 없거나 사운드가 없는 경우 기본 로직 사용
        if (attackPower < 5)
        {
            soundName = "light_hit";
        }
        else if (attackPower < 10)
        {
            soundName = "medium_hit";
        }
        else if (attackPower < 20)
        {
            soundName = "heavy_hit";
        }
        else
        {
            soundName = "critical_hit";
        }
        
        // 해당 효과음이 없으면 기본 효과음 사용
        if (GetAudioClip(soundName) == null)
        {
            soundName = "hitting1";
        }
        
        PlaySoundEffect(soundName, volume);
    }
    
    /// <summary>
    /// 효과 타입에 따른 효과음을 재생합니다. CardSoundData가 있으면 CardSoundData의 설정을 사용합니다.
    /// </summary>
    public void PlayEffectTypeSound(CardEffectType effectType, float volume = 1.0f)
    {
        string soundName;
        
        // CardSoundData가 있으면 CardSoundData의 설정 사용
        if (cardSoundData != null)
        {
            soundName = cardSoundData.GetSoundForEffectType(effectType, true);
            
            // CardSoundData에 설정된 사운드가 있고, 해당 사운드가 로드되어 있으면 사용
            if (!string.IsNullOrEmpty(soundName) && GetAudioClip(soundName) != null)
            {
                PlaySoundEffect(soundName, volume);
                return;
            }
        }
        
        // CardSoundData가 없거나 사운드가 없는 경우 기본 로직 사용
        soundName = GetDefaultSoundNameForEffectType(effectType);
        PlaySoundEffect(soundName, volume);
    }
    
    /// <summary>
    /// 효과 타입에 따른 기본 효과음 이름을 반환합니다.
    /// </summary>
    private string GetDefaultSoundNameForEffectType(CardEffectType effectType)
    {
        string soundName;
        
        switch (effectType)
        {
            case CardEffectType.Damage:
                soundName = "hitting1";
                break;
                
            case CardEffectType.Shield:
                soundName = "defense1";
                break;
                
            case CardEffectType.Draw:
                soundName = "card draw";
                break;
                
            case CardEffectType.Heal:
                soundName = "heal sound";
                break;
                
            case CardEffectType.Poison:
                soundName = "posion debuff";
                break;
                
            case CardEffectType.Weak:
            case CardEffectType.Vulnerable:
                soundName = "etc debuff";
                break;
                
            case CardEffectType.Strength:
                soundName = "buff1";
                break;
                
            case CardEffectType.Critical:
                soundName = "cirtical attack";
                break;
                
            default:
                soundName = "card draw";
                break;
        }
        
        // 해당 효과음이 없으면 기본 효과음 사용
        if (GetAudioClip(soundName) == null)
        {
            soundName = "card draw";
        }
        
        return soundName;
    }
    
    /// <summary>
    /// 특수 효과에 따른 효과음을 재생합니다. CardSoundData가 있으면 CardSoundData의 설정을 사용합니다.
    /// </summary>
    public void PlaySpecialEffectSound(string effectName, float volume = 1.0f)
    {
        string soundName;
        
        // CardSoundData가 있으면 CardSoundData의 설정 사용
        if (cardSoundData != null)
        {
            soundName = cardSoundData.GetSoundForSpecialEffect(effectName, true);
            
            // CardSoundData에 설정된 사운드가 있고, 해당 사운드가 로드되어 있으면 사용
            if (!string.IsNullOrEmpty(soundName) && GetAudioClip(soundName) != null)
            {
                PlaySoundEffect(soundName, volume);
                return;
            }
        }
        
        // CardSoundData가 없거나 사운드가 없는 경우 기본 로직 사용
        switch (effectName.ToLower())
        {
            case "critical":
                soundName = "critical_hit";
                break;
                
            case "weak":
                soundName = "etc debuff";
                break;
                
            case "vulnerable":
                soundName = "etc debuff";
                break;
                
            case "poison":
                soundName = "posion debuff";
                break;
                
            case "buff":
            case "strength":
                soundName = "buff1";
                break;
                
            default:
                soundName = "card draw";
                break;
        }
        
        // 해당 효과음이 없으면 기본 효과음 사용
        if (GetAudioClip(soundName) == null)
        {
            soundName = "card draw";
        }
        
        PlaySoundEffect(soundName, volume);
    }
    
    /// <summary>
    /// CardType에 따른 효과음을 재생합니다. CardSoundData가 있으면 CardSoundData의 설정을 사용합니다.
    /// </summary>
    public void PlayCardTypeSound(object cardType, float volume = 1.0f)
    {
        // CardType 타입인 경우에만 CardSoundData 사용
        if (cardSoundData != null && cardType is CardType cardTypeObj)
        {
            string soundName = cardSoundData.GetSoundForCardType(cardTypeObj, true);
            
            // CardSoundData에 설정된 사운드가 있고, 해당 사운드가 로드되어 있으면 사용
            if (!string.IsNullOrEmpty(soundName) && GetAudioClip(soundName) != null)
            {
                PlaySoundEffect(soundName, volume);
                return;
            }
        }
        
        // 기본 카드 타입 효과음 재생
        PlaySoundEffect("card draw", volume);
    }
    
    /// <summary>
    /// CardSoundData 스크립터블 오브젝트를 설정합니다.
    /// </summary>
    public void SetCardSoundData(CardSoundData newCardSoundData)
    {
        cardSoundData = newCardSoundData;
    }
    
    /// <summary>
    /// 현재 설정된 CardSoundData를 반환합니다.
    /// </summary>
    public CardSoundData GetCardSoundData()
    {
        return cardSoundData;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 효과음 에셋에 Addressables 라벨을 추가합니다.
    /// </summary>
    [MenuItem("Tools/Setup Sound Effect Addressables")]
    public static void SetupSoundEffectAddressables()
    {
        // Addressables 설정 가져오기
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables 설정을 찾을 수 없습니다. Addressables 패키지가 설치되어 있는지 확인하세요.");
            
            // Addressables 설정이 없으면 생성 안내
            if (EditorUtility.DisplayDialog("Addressables 설정 없음", 
                "Addressables 설정이 없습니다. 설정을 생성하시겠습니까?", 
                "설정 생성", "취소"))
            {
                // Addressables 설정 생성 메뉴 실행
                EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
                Debug.Log("Addressables 창이 열렸습니다. 'Create Addressables Settings' 버튼을 클릭하여 설정을 생성하세요.");
            }
            
            return;
        }
        
        // 효과음 데이터 그룹 찾기 또는 생성
        AddressableAssetGroup soundGroup = settings.FindGroup("SoundEffectData");
        if (soundGroup == null)
        {
            // 새 그룹 생성
            soundGroup = settings.CreateGroup("SoundEffectData", false, false, true, 
                new List<AddressableAssetGroupSchema>
                {
                    settings.DefaultGroup.GetSchema<BundledAssetGroupSchema>(),
                    settings.DefaultGroup.GetSchema<ContentUpdateGroupSchema>()
                }, 
                typeof(BundledAssetGroupSchema));
            
            Debug.Log("SoundEffectData 그룹이 생성되었습니다.");
        }
        
        // 효과음 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SOUND_EFFECT_PATH });
        int count = 0;
        
        if (guids.Length == 0)
        {
            Debug.LogError($"'{SOUND_EFFECT_PATH}' 경로에서 AudioClip 에셋을 찾을 수 없습니다. 경로가 올바른지 확인하세요.");
            return;
        }
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // 이미 Addressable인지 확인
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                // Addressable 에셋으로 추가
                entry = settings.CreateOrMoveEntry(guid, soundGroup);
                
                // 라벨 추가
                entry.SetLabel(SOUND_EFFECT_ASSETS_LABEL, true, true);
                
                // 주소 설정 (파일 이름 사용)
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                entry.SetAddress(fileName);
                
                count++;
            }
            else
            {
                // 이미 Addressable이면 라벨만 추가
                if (!entry.labels.Contains(SOUND_EFFECT_ASSETS_LABEL))
                {
                    entry.SetLabel(SOUND_EFFECT_ASSETS_LABEL, true, true);
                    count++;
                }
            }
        }
        
        // 변경사항 저장
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"총 {count}개의 효과음 에셋에 Addressables 라벨이 추가되었습니다.");
        
        // Addressables 빌드 안내
        if (EditorUtility.DisplayDialog("Addressables 설정 완료", 
            "효과음 에셋에 Addressables 라벨이 추가되었습니다. Addressables 빌드를 수행하시겠습니까?", 
            "빌드 수행", "나중에 하기"))
        {
            // Addressables 빌드 메뉴 실행
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            Debug.Log("Addressables 창이 열렸습니다. 'Build > New Build > Default Build Script'를 선택하여 빌드를 수행하세요.");
        }
    }
#endif
}
