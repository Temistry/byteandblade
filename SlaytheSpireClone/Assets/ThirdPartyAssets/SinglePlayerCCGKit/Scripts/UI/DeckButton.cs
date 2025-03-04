// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using System.Collections.Generic;

namespace CCGKit
{
    public class DeckButton : MonoBehaviour
    {
        public Canvas Canvas;
        public CardPileView View;

        public DeckDrawingSystem DeckDrawingSystem;

        public void OnButtonPressed()
        {
            Canvas.gameObject.SetActive(true);
            
            // 게임 중인 경우 DeckDrawingSystem에서 덱 정보를 가져옴
            if (DeckDrawingSystem != null)
            {
                View.AddCards(DeckDrawingSystem.GetDeck());
            }
            // 게임 중이 아닌 경우(로비 등) GameManager에서 소지한 카드 목록을 가져옴
            else
            {
                var cardList = GameManager.GetInstance().GetCardList();
                if (cardList != null && cardList.Count > 0)
                {
                    List<RuntimeCard> runtimeCards = new List<RuntimeCard>();
                    
                    // CardTemplate을 RuntimeCard로 변환
                    foreach (var cardTemplate in cardList)
                    {
                        RuntimeCard runtimeCard = new RuntimeCard
                        {
                            Template = cardTemplate
                        };
                        runtimeCards.Add(runtimeCard);
                    }
                    
                    View.AddCards(runtimeCards);
                }
            }
        }
    }
}