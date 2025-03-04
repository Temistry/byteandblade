// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CCGKit
{
    public class CardPileView : MonoBehaviour
    {
        public GameObject CardPrefab;
        public GameObject Content;

        public HandPresentationSystem HandPresentationSystem;

        private List<GameObject> widgets = new List<GameObject>(16);

        private void Start()
        {

        }

        public void AddCards(List<RuntimeCard> cards)
        {
            var sortedCards = cards.OrderBy(x => x.Template.Id).ToList();
            foreach (var card in sortedCards)
            {
                var widget = Instantiate(CardPrefab);
                widget.transform.SetParent(Content.transform, false);
                widget.GetComponent<CardWidget>().SetInfo(card);
                widget.gameObject.SetActive(true);
                widgets.Add(widget);
            }
        }

        private void OnEnable()
        {
            foreach (var widget in widgets)
            {
                Destroy(widget);
            }

            widgets.Clear();

            if (HandPresentationSystem != null)
            {
                HandPresentationSystem.SetHandCardsInteractable(false);
            }
        }

        private void OnDisable()
        {
            if (HandPresentationSystem != null)
            {
                HandPresentationSystem.SetHandCardsInteractable(true);
            }
        }
    }
}