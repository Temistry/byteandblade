using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_PlayPannel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] GameObject relicsRow;
    [SerializeField] TextMeshProUGUI playTimeText;
    
    void Awake()
    {
        GameManager.GetInstance().OnHealthChanged += UpdateHealthText;
        GameManager.GetInstance().OnGoldChanged += UpdateGoldText;
        GameManager.GetInstance().OnMaxHealthChanged += UpdateMaxHealthText;
        GameManager.GetInstance().OnPlayTimeChanged += UpdatePlayTimeText;
    }

    void Start()
    {       
        // 게임 씬이 아니면 플레이 시간 갱신 이벤트 제거
        if(SceneManager.GetActiveScene().name != "1.Game")
        {
            GameManager.GetInstance().OnPlayTimeChanged -= UpdatePlayTimeText;
            playTimeText.text = "00:00:00";
        }
    }
    
    void OnDestroy()
    {
        GameManager.GetInstance().OnHealthChanged -= UpdateHealthText;
        GameManager.GetInstance().OnGoldChanged -= UpdateGoldText;
        GameManager.GetInstance().OnMaxHealthChanged -= UpdateMaxHealthText;
        GameManager.GetInstance().OnPlayTimeChanged -= UpdatePlayTimeText;
    }

    void UpdateHealthText()
    {
        healthText.text = GameManager.GetInstance().Health.ToString();
    }

    void UpdateGoldText()
    {
        goldText.text = GameManager.GetInstance().Gold.ToString();
    }

    void UpdateMaxHealthText()
    {
        healthText.text = GameManager.GetInstance().MaxHealth.ToString();
    }   

    void UpdatePlayTimeText()
    {
        playTimeText.text = GameManager.GetInstance().PlayTimeString;
    }

}
