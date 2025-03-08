using UnityEngine;
using CCGKit; // Node와 NodeType 클래스가 있는 네임스페이스

public class GoldRewardSystem : MonoBehaviour
{
    public void AddGold(int amount)
    {
        GameManager.GetInstance().AddGold(amount);
        Debug.Log($"GoldRewardSystem : {amount} 추가");
        GameManager.GetInstance().Save();
    }

    public void OnPlayerRedeemedReward()
    {
        /*
        1~30 : 일반
        30~60 : 엘리트
        60~100 : 보스
        */
        int amount = 0;

        // 현재 클리어한 노드 가져오기
        Node currentNode = GameManager.GetInstance().GetCurrentNode();
        if (currentNode == null)
        {
            return;
        }

        // 보스 노드를 클리어했는지 확인
        if (currentNode.Type == NodeType.Boss)
        {   
            amount = Random.Range(60, 100);
        }
        // 엘리트 노드를 클리어했는지 확인
        else if (currentNode.Type == NodeType.Elite)
        {
            amount = Random.Range(30, 60);
        }
        // 일반 노드를 클리어했는지 확인
        else
        {
            amount = Random.Range(1, 30);
        }   

        AddGold(amount);
    }
}
