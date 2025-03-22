using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬 내의 모든 UI 버튼에 자동으로 사운드를 추가하는 컴포넌트
/// </summary>
public class ButtonSoundInitializer : MonoBehaviour
{
    [Tooltip("사용할 클릭 효과음 이름")]
    public string clickSoundName = "card draw";
    
    [Tooltip("모든 버튼에 자동으로 사운드 추가")]
    public bool autoAddToAllButtons = true;
    
    private void Start()
    {
        // 효과음 시스템이 초기화될 때까지 잠시 대기
        Invoke("CheckAndAddSounds", 0.5f);
    }
    
    private void CheckAndAddSounds()
    {
        if (Parser_EffectSound.Instance != null && Parser_EffectSound.Instance.IsSoundEffectLoadingComplete())
        {
            if (autoAddToAllButtons)
            {
                AddSoundToAllButtons();
            }
        }
        else
        {
            // 아직 로드 중이면 다시 시도
            Invoke("CheckAndAddSounds", 0.5f);
        }
    }
    
    /// <summary>
    /// 씬 내의 모든 버튼에 효과음을 추가합니다.
    /// </summary>
    public void AddSoundToAllButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        int addedCount = 0;
        
        foreach (Button button in buttons)
        {
            // 이미 효과음이 할당되어 있는지 확인 (중복 방지)
            bool hasSound = false;
            int eventCount = button.onClick.GetPersistentEventCount();
            
            // 이미 이벤트가 등록된 경우 사운드 이벤트가 있는지 확인
            if (eventCount > 0)
            {
                // 이벤트 핸들러 이름 검사는 에디터에서만 가능하므로
                // 여기서는 이벤트가 있다면 이미 사운드가 설정되어 있다고 가정
                hasSound = true;
            }
            
            if (!hasSound)
            {
                SoundEffectHelper.AddButtonClickSound(button, clickSoundName);
                addedCount++;
            }
        }
        
        Debug.Log($"총 {addedCount}개의 버튼에 효과음이 추가되었습니다 (전체 버튼: {buttons.Length}개).");
    }
    
    /// <summary>
    /// 특정 버튼에 효과음을 추가합니다.
    /// </summary>
    public void AddSoundToButton(Button button)
    {
        if (button != null)
        {
            SoundEffectHelper.AddButtonClickSound(button, clickSoundName);
        }
    }
} 