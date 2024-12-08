using UnityEngine;
using UnityEngine.UI;

public class UI_StartRelic : MonoBehaviour
{

    public Button[] relicButtons;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateRandomRelicButtons();

        for (int i = 0; i < relicButtons.Length; i++)
        {
            int relicIndex = i;
            relicButtons[relicIndex].onClick.AddListener(() => OnRelicButtonClick(relicIndex));
        }
    }

    public StartRelicData relicData; // ScriptableObject를 참조하는 변수

    void GenerateRandomRelicButtons()
    {
        // relicData에서 랜덤으로 선택된 버튼의 정보를 가져옵니다.
        int[] randomRelicIndices = relicData.GetRandomRelicIndices(relicButtons.Length);

        // 선택된 버튼만 활성화합니다.
        foreach (Button button in relicButtons)
        {
            button.gameObject.SetActive(false);
        }

        foreach (int index in randomRelicIndices)
        {
            relicButtons[index].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnRelicButtonClick(int relicIndex)
    {
        Debug.Log("Relic button clicked: " + relicIndex);
    }
}
