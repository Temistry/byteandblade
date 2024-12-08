// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The base class of the card selection systems. It contains the shared code of both
    /// selection systems (with and without a targeting arrow).
    /// </summary>
    public abstract class CardSelectionSystem : BaseSystem
    {
        public IntVariable PlayerMana;

        public TurnManagementSystem TurnManagementSystem;
        public DeckDrawingSystem DeckDrawingSystem;
        public HandPresentationSystem HandPresentationSystem;
        public EffectResolutionSystem EffectResolutionSystem;

        protected Camera MainCamera;
        protected LayerMask CardLayer;
        protected LayerMask EnemyLayer;

        protected GameObject SelectedCard;

        protected virtual void Start()
        {
            CardLayer = 1 << LayerMask.NameToLayer("Card");
            EnemyLayer = 1 << LayerMask.NameToLayer("Enemy");
            MainCamera = Camera.main;
        }

        protected virtual void PlaySelectedCard()
        {
            var cardObject = SelectedCard.GetComponent<CardObject>();
            cardObject.SetInteractable(false);
            var cardTemplate = cardObject.Template;
            PlayerMana.SetValue(PlayerMana.Value - cardTemplate.Cost);

            HandPresentationSystem.RearrangeHand(SelectedCard);
            HandPresentationSystem.RemoveCardFromHand(SelectedCard);
            HandPresentationSystem.MoveCardToDiscardPile(SelectedCard);

            DeckDrawingSystem.MoveCardToDiscardPile(cardObject.RuntimeCard);
        }

        public bool HasSelectedCard()
        {
            return SelectedCard != null;
        }
    }
}
