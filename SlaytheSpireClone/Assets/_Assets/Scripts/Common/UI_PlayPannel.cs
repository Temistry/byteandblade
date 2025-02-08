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
        BindEvent();
    }
    
    public void BindEvent()
    {
        GameManager.GetInstance().OnHealthChanged -= UpdateHealthText;
        GameManager.GetInstance().OnHealthChanged += UpdateHealthText;
        GameManager.GetInstance().OnGoldChanged -= UpdateGoldText;
        GameManager.GetInstance().OnGoldChanged += UpdateGoldText;
        GameManager.GetInstance().OnMaxHealthChanged -= UpdateMaxHealthText;
        GameManager.GetInstance().OnMaxHealthChanged += UpdateMaxHealthText;
        GameManager.GetInstance().OnPlayTimeChanged -= UpdatePlayTimeText;
        GameManager.GetInstance().OnPlayTimeChanged += UpdatePlayTimeText;

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
