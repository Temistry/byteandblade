// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace CCGKit
{
    /// <summary>
    /// This component is attached to the EndBattlePopup prefab.
    /// </summary>
    public class EndBattlePopup : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private Button continueButton;
        [SerializeField]
        private Button rewardButton;
        [SerializeField]
        private Button goHomeButton;

        [SerializeField]
        private GameEvent cardRewardEvent;
        [SerializeField]
        private Canvas popupCanvas;
#pragma warning restore 649

        private CanvasGroup canvasGroup;

        private const string VictoryText = "Victory";
        private const string DefeatText = "Defeat";
        private const string DefeatDescriptionText = "The dungeon run was too hard this time... better luck next time!";
        private const float FadeInTime = 0.4f;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1.0f, FadeInTime);
        }

        public void SetVictoryText()
        {
            titleText.text = VictoryText;
            descriptionText.text = string.Empty;
            goHomeButton.gameObject.SetActive(false);
        }

        public void SetDefeatText()
        {
            Destroy(rewardButton.gameObject);
            titleText.text = DefeatText;
            descriptionText.text = DefeatDescriptionText;
            continueButton.gameObject.SetActive(false);
            goHomeButton.gameObject.SetActive(true);
        }

        public void OnGoHomeButtonPressed()
        {
            Transition.LoadLevel("0.Lobby", 0.5f, Color.black);
        }

        public void OnContinueButtonPressed()
        {
            Transition.LoadLevel("0.Lobby", 0.5f, Color.black);
        }

        public void OnCardRewardButtonPressed()
        {
            Destroy(rewardButton.gameObject);
            popupCanvas.gameObject.SetActive(false);
            cardRewardEvent.Raise();
        }
    }
}
