using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
