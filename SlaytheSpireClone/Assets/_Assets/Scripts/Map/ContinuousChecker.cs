using UnityEngine;

public class ContinuousChecker : MonoBehaviour
{
    [SerializeField] GameObject startRelicUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 이어서 하는 경우 시작 유물 선택창 띄우지 않음
        if(GameManager.GetInstance().IsContinueGame())
        {
            startRelicUI.SetActive(false);
            return;
        }

    }
}
