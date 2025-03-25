// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCGKit
{
    public class CardRewardWidget : MonoBehaviour
    {
        public Canvas Canvas;
        public Canvas PopupCanvas;
        public CardWidget cardWidget;
        public bool isRewardMode = false;

        private void Awake()
        {
            cardWidget = GetComponent<CardWidget>();
        }

        public void OnCardPressed()
        {
            if(null == cardWidget.Card)
            {
                Debug.LogWarning("카드 위젯의 카드가 null입니다.");
                return;
            }

            var gameInfo = FindFirstObjectByType<GameInfo>();
            if(gameInfo == null)
            {
                Debug.LogWarning("GameInfo 컴포넌트를 찾을 수 없습니다.");
                return;
            }
            
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"현재 씬: {currentScene}");
            
            if(currentScene == "2.Game")
            {
                if (isRewardMode)
                {
                    Debug.Log($"카드 '{cardWidget.Card.Name}'을(를) 덱에 추가합니다.");
                    GameManager.GetInstance().AddCard(cardWidget.Card);
                    GameManager.GetInstance().Save();
                    Canvas.gameObject.SetActive(false);
                    PopupCanvas.gameObject.SetActive(true);
                    Debug.Log("카드가 성공적으로 추가되었습니다.");
                }
                else
                {
                    Debug.Log($"카드 '{cardWidget.Card.Name}'을(를) 조회합니다.");
                }
            }
            else
            {
                Debug.LogWarning("현재 씬이 '2.Game'이 아니므로 카드를 추가할 수 없습니다.");
            }
        }
    }
}