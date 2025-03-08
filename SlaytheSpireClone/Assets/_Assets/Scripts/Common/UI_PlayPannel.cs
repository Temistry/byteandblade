using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UI_PlayPannel : MonoBehaviour
{
    public TextMeshProUGUI nickNameText;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] TextMeshProUGUI playTimeText;
    
    void Awake()
    {
        // GameManager가 초기화될 때까지 대기
        if (GameManager.GetInstance() != null)
        {
            BindEvent();
        }
        else
        {
            // 다음 프레임에서 다시 시도
            StartCoroutine(WaitForGameManager());
        }
    }

    private IEnumerator WaitForGameManager()
    {
        while (GameManager.GetInstance() == null)
        {
            yield return null;
        }
        BindEvent();
    }

    public void BindEvent()
    {
        GameManager.GetInstance().OnHealthChanged -= UpdateHealthText;
        GameManager.GetInstance().OnHealthChanged += UpdateHealthText;
        GameManager.GetInstance().OnGoldChanged -= UpdateGoldText;
        GameManager.GetInstance().OnGoldChanged += UpdateGoldText;
        GameManager.GetInstance().OnPlayTimeChanged -= UpdatePlayTimeText;
        GameManager.GetInstance().OnPlayTimeChanged += UpdatePlayTimeText;
        GameManager.GetInstance().OnRegiserNickName -= UpdateNickNameText;
        GameManager.GetInstance().OnRegiserNickName += UpdateNickNameText;


        // 게임 씬이 아니면 플레이 시간 갱신 이벤트 제거
        if(SceneManager.GetActiveScene().name != "2.Game")
        {
            GameManager.GetInstance().OnPlayTimeChanged -= UpdatePlayTimeText;
            playTimeText.text = "";
        }
    }

    void Start()
    {       
        
    }
    
    void OnDestroy()
    {
        GameManager.GetInstance().OnHealthChanged -= UpdateHealthText;
        GameManager.GetInstance().OnGoldChanged -= UpdateGoldText;
        GameManager.GetInstance().OnPlayTimeChanged -= UpdatePlayTimeText;
        GameManager.GetInstance().OnRegiserNickName -= UpdateNickNameText;
    }

    void UpdateNickNameText(string nickName)
    {
        nickNameText.text = nickName;
    }

    void UpdateHealthText()
    {
        healthText.text =  "HP " + GameManager.GetInstance().MaxHealth.ToString() + " / " + GameManager.GetInstance().Health.ToString();
    }

    void UpdateGoldText()
    {
        goldText.text = GameManager.GetInstance().Gold.ToString() + "G";
    }

    void UpdatePlayTimeText(string playTime)
    {
        playTimeText.text = playTime;
    }

    // 모든 UI 요소를 강제로 업데이트하는 메서드
    public void UpdateUI()
    {
        var gameManager = GameManager.GetInstance();
        if (gameManager != null)
        {
            // 닉네임 업데이트
            if (nickNameText != null)
            {
                UpdateNickNameText(gameManager.NickName);
            }
            
            // 체력 업데이트
            if (healthText != null)
            {
                UpdateHealthText();
            }
            
            // 골드 업데이트
            if (goldText != null)
            {
                UpdateGoldText();
            }
            
            // 플레이 시간 업데이트
            if (playTimeText != null)
            {
                UpdatePlayTimeText(playTimeText.text);
            }
            
            Debug.Log("UI_PlayPannel 텍스트가 업데이트되었습니다.");
        }
    }
}
