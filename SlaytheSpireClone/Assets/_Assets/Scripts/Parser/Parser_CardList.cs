using System.Collections.Generic;
using CCGKit;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

public class Parser_CardList : Singleton<Parser_CardList>
{
    private List<CardTemplate> cardList = new List<CardTemplate>();
    
    // Addressables 라벨 (카드 에셋에 이 라벨을 추가해야 함)
    private const string CARD_ASSETS_LABEL = "CardData";
    // 카드 데이터 경로
    private const string CARD_DATA_PATH = "Assets/_Assets/GameData/Cards";

    // 로드 상태 추적
    private bool isLoading = false;
    private bool loadFailed = false;
    
    // 로드 완료 이벤트
    public delegate void CardsLoadedHandler(bool success, int cardCount, string errorMessage);
    public event CardsLoadedHandler OnCardsLoaded;

    public override void Awake()
    {
        base.Awake();
        LoadAllCardsAsync();
    }

    /// <summary>
    /// ID로 카드 템플릿을 찾습니다.
    /// </summary>
    public CardTemplate GetCardTemplate(int id)
    {
        return cardList.Find(card => card.Id == id);
    }

    /// <summary>
    /// 모든 카드 데이터를 비동기적으로 로드합니다.
    /// </summary>
    public async void LoadAllCardsAsync()
    {
        // 이미 로드 중이면 중복 로드 방지
        if (isLoading)
        {
            Debug.LogWarning("카드 데이터가 이미 로드 중입니다.");
            return;
        }

        isLoading = true;
        loadFailed = false;
        cardList.Clear();
        
        AsyncOperationHandle initOperation = default;
        AsyncOperationHandle<IList<IResourceLocation>> checkLabelOperation = default;
        AsyncOperationHandle<IList<CardTemplate>> loadOperation = default;
        
        try
        {
            // Addressables 시스템 초기화
            Debug.Log("Addressables 시스템 초기화 중...");

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
            Debug.Log($"'{CARD_ASSETS_LABEL}' 라벨 확인 중...");
            checkLabelOperation = Addressables.LoadResourceLocationsAsync(CARD_ASSETS_LABEL);
            await checkLabelOperation.Task;
            
            if (checkLabelOperation.Status == AsyncOperationStatus.Failed)
            {
                throw new System.Exception("라벨 확인 실패: " + checkLabelOperation.OperationException?.Message);
            }
            
            IList<IResourceLocation> locations = checkLabelOperation.Result;
            if (locations == null || locations.Count == 0)
            {
                throw new System.Exception($"'{CARD_ASSETS_LABEL}' 라벨을 가진 에셋이 없습니다. 'Tools > Setup Card Addressables' 메뉴를 실행하여 카드 에셋에 라벨을 추가하세요.");
            }
            
            Debug.Log($"'{CARD_ASSETS_LABEL}' 라벨을 가진 에셋 {locations.Count}개를 찾았습니다. 로드 시작...");
            
            // 라벨로 모든 카드 에셋 로드 (콜백 방식 대신 결과 리스트를 직접 받는 방식으로 변경)
            loadOperation = Addressables.LoadAssetsAsync<CardTemplate>(
                CARD_ASSETS_LABEL, 
                null);  // 콜백 제거
            
            // 로드 완료 대기
            await loadOperation.Task;
            
            if (loadOperation.Status == AsyncOperationStatus.Failed)
            {
                throw new System.Exception("카드 에셋 로드 실패: " + loadOperation.OperationException?.Message);
            }
            
            // 결과를 cardList에 추가
            var loadedCards = loadOperation.Result;
            if (loadedCards != null)
            {
                foreach (var card in loadedCards)
                {
                    if (card != null)
                    {
                        cardList.Add(card);
                        Debug.Log($"카드 로드: {card.Name} (ID: {card.Id})");
                    }
                }
            }
            
            if (cardList.Count == 0)
            {
                throw new System.Exception("카드가 로드되지 않았습니다. 카드 에셋이 올바르게 설정되었는지 확인하세요.");
            }
            
            // 카드 ID 순으로 정렬
            cardList = cardList.OrderBy(card => card.Id).ToList();
            
            Debug.Log($"총 {cardList.Count}개의 카드 데이터를 로드했습니다.");
            
            // 로드 완료 이벤트 발생
            OnCardsLoaded?.Invoke(true, cardList.Count, null);
        }
        catch (System.Exception e)
        {
            loadFailed = true;
            string errorMessage = $"카드 데이터 로드 중 오류 발생: {e.Message}";
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
                Debug.LogError("해결 방법: 'Tools > Setup Card Addressables' 메뉴를 실행하여 카드 에셋에 라벨을 추가하세요. 그 후 Window > Asset Management > Addressables > Groups 메뉴에서 Build > New Build > Default Build Script를 실행하세요.");
            }
            else if (e.Message.Contains("카드 에셋 로드 실패"))
            {
                Debug.LogError("해결 방법: 카드 에셋이 유효한지 확인하세요. 손상된 에셋이 있을 수 있습니다. 또한 Addressables 빌드가 최신 상태인지 확인하세요.");
            }
            else if (e.Message.Contains("카드가 로드되지 않았습니다"))
            {
                Debug.LogError("해결 방법: 카드 에셋이 올바른 형식(CardTemplate)인지 확인하고, 'Tools > Setup Card Addressables' 메뉴를 다시 실행한 후 Addressables 빌드를 수행하세요.");
            }
            else if (e.Message.Contains("invalid operation handle"))
            {
                Debug.LogError("해결 방법: Addressables 빌드를 다시 수행하세요. Window > Asset Management > Addressables > Groups 메뉴에서 Build > Clean > All을 선택한 후, Build > New Build > Default Build Script를 실행하세요.");
            }
            
            // 로드 실패 이벤트 발생
            OnCardsLoaded?.Invoke(false, 0, errorMessage);
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
            
            // 로드 작업 핸들은 카드 데이터가 필요한 동안 유지해야 할 수 있으므로,
            // 실패한 경우에만 해제
            if (loadFailed && loadOperation.IsValid())
            {
                Addressables.Release(loadOperation);
            }
            
            isLoading = false;
        }
    }

    /// <summary>
    /// 카드 로드 상태를 확인합니다.
    /// </summary>
    public bool IsCardLoadingComplete()
    {
        return !isLoading;
    }
    
    /// <summary>
    /// 카드 로드가 실패했는지 확인합니다.
    /// </summary>
    public bool HasCardLoadingFailed()
    {
        return loadFailed;
    }
    
    /// <summary>
    /// 카드 로드를 다시 시도합니다.
    /// </summary>
    public void RetryCardLoading()
    {
        if (!isLoading)
        {
            LoadAllCardsAsync();
        }
    }

    /// <summary>
    /// 이름으로 카드 템플릿을 찾습니다.
    /// </summary>
    public CardTemplate GetCardTemplateByName(string name)
    {
        return cardList.Find(card => card.Name == name);
    }

    /// <summary>
    /// 특정 ID 범위의 카드 템플릿을 반환합니다.
    /// </summary>
    public List<CardTemplate> GetCardTemplatesInRange(int minId, int maxId)
    {
        return cardList.FindAll(card => card.Id >= minId && card.Id <= maxId);
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 카드 에셋에 Addressables 라벨을 추가합니다.
    /// </summary>
    [MenuItem("Tools/Setup Card Addressables")]
    public static void SetupCardAddressables()
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
        
        // 카드 데이터 그룹 찾기 또는 생성
        AddressableAssetGroup cardGroup = settings.FindGroup("CardData");
        if (cardGroup == null)
        {
            // 새 그룹 생성
            cardGroup = settings.CreateGroup("CardData", false, false, true, 
                new List<AddressableAssetGroupSchema>
                {
                    settings.DefaultGroup.GetSchema<BundledAssetGroupSchema>(),
                    settings.DefaultGroup.GetSchema<ContentUpdateGroupSchema>()
                }, 
                typeof(BundledAssetGroupSchema));
            
            Debug.Log("CardData 그룹이 생성되었습니다.");
        }
        
        // 카드 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("t:CardTemplate", new[] { CARD_DATA_PATH });
        int count = 0;
        
        if (guids.Length == 0)
        {
            Debug.LogError($"'{CARD_DATA_PATH}' 경로에서 CardTemplate 에셋을 찾을 수 없습니다. 경로가 올바른지 확인하세요.");
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
                entry = settings.CreateOrMoveEntry(guid, cardGroup);
                
                // 라벨 추가
                entry.SetLabel(CARD_ASSETS_LABEL, true, true);
                
                // 주소 설정 (파일 이름 사용)
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                entry.SetAddress(fileName);
                
                count++;
            }
            else
            {
                // 이미 Addressable이면 라벨만 추가
                if (!entry.labels.Contains(CARD_ASSETS_LABEL))
                {
                    entry.SetLabel(CARD_ASSETS_LABEL, true, true);
                    count++;
                }
            }
        }
        
        // 변경사항 저장
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"총 {count}개의 카드 에셋에 Addressables 라벨이 추가되었습니다.");
        
        // Addressables 빌드 안내
        if (EditorUtility.DisplayDialog("Addressables 설정 완료", 
            "카드 에셋에 Addressables 라벨이 추가되었습니다. Addressables 빌드를 수행하시겠습니까?", 
            "빌드 수행", "나중에 하기"))
        {
            // Addressables 빌드 메뉴 실행
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            Debug.Log("Addressables 창이 열렸습니다. 'Build > New Build > Default Build Script'를 선택하여 빌드를 수행하세요.");
        }
    }
#endif
}
