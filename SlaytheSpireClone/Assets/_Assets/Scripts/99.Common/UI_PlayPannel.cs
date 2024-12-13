using UnityEngine;
using UnityEngine.UIElements;
using System;

public class UI_PlayPannel : MonoBehaviour
{
    private VisualElement root;
    private Label playTimeLabel;
    private float playTime = 0f;
    private VisualElement relicsRow;
    
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        playTimeLabel = root.Q<Label>("play-time");
        relicsRow = root.Q<VisualElement>("relics-row");
        
        // 버튼 이벤트 등록
        RegisterButtonEvents();
        
        // 디버그용 유물 추가
        AddDebugRelics();
    }

    void AddDebugRelics()
    {
        // 디버그용 유물 5개 추가
        for (int i = 0; i < 5; i++)
        {
            var relicItem = new VisualElement();
            relicItem.AddToClassList("relic-item");
            
            var relicImage = new VisualElement();
            relicImage.AddToClassList("relic-image");
            // 디버그용 이미지 - 실제로는 유물 데이터의 아이콘을 사용
            relicImage.style.backgroundColor = new Color(
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                1
            );
            
            relicItem.Add(relicImage);
            relicsRow.Add(relicItem);
            
            // 툴팁 이벤트 추가
            int relicIndex = i; // 클로저를 위한 복사
            relicItem.RegisterCallback<MouseEnterEvent>((evt) => {
                Debug.Log($"유물 {relicIndex + 1} 정보: 디버그 유물");
            });
        }
    }

    void Update()
    {
        // 플레이 시간 업데이트
        playTime += Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(playTime);
        playTimeLabel.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    void RegisterButtonEvents()
    {
        // 물약 버튼
        root.Q<Button>("potion1").clicked += () => Debug.Log("물약 1 사용");
        root.Q<Button>("potion2").clicked += () => Debug.Log("물약 2 사용");
        root.Q<Button>("potion3").clicked += () => Debug.Log("물약 3 사용");
        
        // 기능 버튼
        root.Q<Button>("map-button").clicked += () => Debug.Log("지도 열기");
        root.Q<Button>("deck-button").clicked += () => Debug.Log("덱 보기");
        root.Q<Button>("settings-button").clicked += () => Debug.Log("설정 메뉴");
    }
}
