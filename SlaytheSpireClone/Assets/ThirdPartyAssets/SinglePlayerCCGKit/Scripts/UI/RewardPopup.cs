using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CCGKit
{
    public class RewardPopup : MonoBehaviour
    {
        [SerializeField] private GameObject rewardCardPrefab;
        [SerializeField] private Transform rewardCardsContainer;
        [SerializeField] private Button continueButton;

        public void Show(List<CardTemplate> rewardCards)
        {
            gameObject.SetActive(true);
            
            // 보상 카드 생성
            foreach (var card in rewardCards)
            {
                var cardObj = Instantiate(rewardCardPrefab, rewardCardsContainer);
                // 카드 정보 설정
            }

            continueButton.onClick.AddListener(() => {
                // 선택된 카드를 덱에 추가
                // 로비로 이동
                Transition.LoadLevel("0.Lobby", 1.0f, Color.black);
            });
        }
    }
} 