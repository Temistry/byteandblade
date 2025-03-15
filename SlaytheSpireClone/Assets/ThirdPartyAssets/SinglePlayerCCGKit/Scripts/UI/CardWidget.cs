// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CCGKit
{
    public class CardWidget : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private TextMeshProUGUI costText;
        [SerializeField]
        private TextMeshProUGUI nameText;
        [SerializeField]
        private TextMeshProUGUI typeText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private Image picture;
#pragma warning restore 649

        public CardTemplate Card;

        public void SetInfo(RuntimeCard card)
        {
            SetInfo(card.Template);
        }

        public void SetInfo(CardTemplate template)
        {
            Card = template;

            costText.text = template.Cost.ToString();
            nameText.text = LanguageManager.GetText(template.Name);
            typeText.text = LanguageManager.GetText(template.Type.name);
            
            string translateText = "";
            foreach (var effect in template.Effects)
            {
                string effectName = effect.GetName();
                var builder = new StringBuilder();
                builder.AppendFormat("{0}", effectName);
                translateText += LanguageManager.GetText(builder.ToString()) + ". ";
            }
            descriptionText.text = translateText;
            picture.sprite = template.Picture;
        }

        public CardTemplate GetInfo()
        {
            return Card;
        }
    }
}
