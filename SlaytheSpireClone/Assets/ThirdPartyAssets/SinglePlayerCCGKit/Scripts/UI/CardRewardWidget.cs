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

        private CardWidget cardWidget;

        private void Awake()
        {
            cardWidget = GetComponent<CardWidget>();
        }

        public void OnCardPressed()
        {
            if(null == cardWidget.Card)
                return;

            var gameInfo = FindFirstObjectByType<GameInfo>();
            if(gameInfo == null)
                return;
            
            if(SceneManager.GetActiveScene().name == "2.Game")
            {
                GameManager.GetInstance().AddCard(cardWidget.Card);
                GameManager.GetInstance().Save();
                Canvas.gameObject.SetActive(false);
                PopupCanvas.gameObject.SetActive(true);
            }
        }
    }
}