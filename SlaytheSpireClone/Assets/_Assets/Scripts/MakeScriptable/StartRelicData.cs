using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "RelicData", menuName = "Game Data/Start Relic")]
public class StartRelicData : ScriptableObject
{
    public int numberOfChoices = 3;      // 선택지 수
    public int[] rewardKeys;         // 각 선택지별 보상 키 값
    
    public string relicName;
    public string description;
    public Sprite icon;
    public int power;

    public int[] GetRandomRelicIndices(int count)
    {
        if (rewardKeys == null || rewardKeys.Length == 0)
        {
            Debug.LogError("보상 키 값이 설정되지 않았습니다!");
            return new int[0];
        }
        return Enumerable.Range(0, count).OrderBy(x => Guid.NewGuid()).ToArray();
    }
    
}
