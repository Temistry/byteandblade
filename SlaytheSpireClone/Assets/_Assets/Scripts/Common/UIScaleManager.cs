using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CCGKit;

[System.Serializable]
public class SceneScaleSettings
{
    public string sceneName;
    public float scaleFactor = 0.8f;
    public float cardScaleFactor = 0.3f;
    public float hpWidgetScaleFactor = 0.7f;  // HP 위젯 크기 조정을 위한 스케일 팩터 추가
    public Vector2 referenceResolution = new Vector2(1280, 800);
}

public class UIScaleManager : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    private Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
    
    [SerializeField] private SceneScaleSettings[] sceneSettings;
    private SceneScaleSettings currentSettings;

    public static UIScaleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            // 현재 씬에 맞는 설정 찾기
            string currentScene = SceneManager.GetActiveScene().name;
            currentSettings = GetSceneSettings(currentScene);
            
            // UI 스케일 조정
            ApplyUIScale();
        }
    }

    private SceneScaleSettings GetSceneSettings(string sceneName)
    {
        // 씬별 설정 찾기
        foreach (var settings in sceneSettings)
        {
            if (settings.sceneName == sceneName)
            {
                return settings;
            }
        }
        
        // 기본 설정 반환
        return new SceneScaleSettings
        {
            sceneName = "Default",
            scaleFactor = 0.8f,
            cardScaleFactor = 0.3f,
            hpWidgetScaleFactor = 0.7f,
            referenceResolution = new Vector2(1280, 800)
        };
    }

    private void ApplyUIScale()
    {
        if (currentSettings == null) return;

        // CanvasScaler 설정 적용
        canvasScaler.referenceResolution = currentSettings.referenceResolution;
        canvasScaler.scaleFactor = currentSettings.scaleFactor;
        
        // 모든 UI 요소들의 크기 조정
        AdjustUIElements();
    }

    // HP 위젯 스케일링을 위한 public 메서드
    public void ScaleHPWidget(RectTransform hpWidgetTransform)
    {
        if (currentSettings == null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            currentSettings = GetSceneSettings(currentScene);
        }

        if (hpWidgetTransform != null)
        {
            hpWidgetTransform.localScale = Vector3.one * currentSettings.hpWidgetScaleFactor;
        }
    }

    private void AdjustUIElements()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // 모든 UI 요소들을 찾아서 크기 조정
        var uiElements = FindObjectsOfType<RectTransform>();
        foreach (var element in uiElements)
        {
            // HP 위젯 크기 조정
            var hpWidget = element.GetComponent<HpWidget>();
            if (hpWidget != null)
            {
                element.localScale = Vector3.one * currentSettings.hpWidgetScaleFactor;
                continue;
            }

            // 게임 씬에서만 카드 관련 UI 요소들 특별 처리
            if (currentScene == "2.Game" && (element.GetComponent<CardWidget>() != null || element.GetComponent<DeckWidget>() != null))
            {
                element.localScale = new Vector3(currentSettings.cardScaleFactor, currentSettings.cardScaleFactor, 1f);
            }
            else
            {
                // 일반 UI 요소들 처리
                // 텍스트 크기 조정
                var text = element.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = Mathf.RoundToInt(text.fontSize * currentSettings.scaleFactor);
                }
                
                // 버튼 크기 조정
                var button = element.GetComponent<Button>();
                if (button != null)
                {
                    var buttonRect = button.GetComponent<RectTransform>();
                    buttonRect.sizeDelta *= currentSettings.scaleFactor;
                }
            }
        }
    }

    // 씬이 변경될 때 호출되는 메서드
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬의 설정 적용
        currentSettings = GetSceneSettings(scene.name);
        ApplyUIScale();
    }
} 