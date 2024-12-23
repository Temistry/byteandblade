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
        GameManager.instance.OnHealthChanged += UpdateHealthText;
        GameManager.instance.OnGoldChanged += UpdateGoldText;
        GameManager.instance.OnMaxHealthChanged += UpdateMaxHealthText;
        GameManager.instance.OnPlayTimeChanged += UpdatePlayTimeText;
    }

    void Start()
    {       
       
    }
    
    void OnDestroy()
    {
        GameManager.instance.OnHealthChanged -= UpdateHealthText;
        GameManager.instance.OnGoldChanged -= UpdateGoldText;
        GameManager.instance.OnMaxHealthChanged -= UpdateMaxHealthText;
        GameManager.instance.OnPlayTimeChanged -= UpdatePlayTimeText;
    }

    void UpdateHealthText()
    {
        healthText.text = GameManager.instance.Health.ToString();
    }

    void UpdateGoldText()
    {
        goldText.text = GameManager.instance.Gold.ToString();
    }

    void UpdateMaxHealthText()
    {
        healthText.text = GameManager.instance.MaxHealth.ToString();
    }   

    void UpdatePlayTimeText()
    {
        playTimeText.text = GameManager.instance.PlayTimeString;
    }

}
