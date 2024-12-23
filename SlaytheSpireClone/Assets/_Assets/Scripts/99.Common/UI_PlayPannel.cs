using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayPannel : MonoBehaviour
{


    [SerializeField] GameObject relicsRow;
    [SerializeField] TextMeshProUGUI playTimeText;
    
    void Start()
    {       
        // 버튼 이벤트 등록
        RegisterButtonEvents();
        

    }

    void Update()
    {
        // 플레이 시간 업데이트
        playTimeText.text = GameManager.instance.UpdatePlayTime();
    }

    void RegisterButtonEvents()
    {
       

    }
}
