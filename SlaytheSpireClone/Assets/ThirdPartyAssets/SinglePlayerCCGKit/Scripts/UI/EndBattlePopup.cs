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
        private Button characterRewardButton;
        [SerializeField]
        private Button goldRewardButton;
        [SerializeField]
        private Button goHomeButton;

        [SerializeField]
        private GameEvent cardRewardEvent;

        [SerializeField]
        private GameEvent characterRewardEvent;

        [SerializeField]
        private GameEvent goldRewardEvent;

        [SerializeField]
        private Canvas popupCanvas;
#pragma warning restore 649

        private CanvasGroup canvasGroup;

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

        public void ShowVictory()
        {
            titleText.text = LanguageManager.GetText("Victory");
            descriptionText.text = string.Empty;
            SetButtonsForNormalVictory();
        }

        public void ShowDefeat()
        {
            titleText.text = LanguageManager.GetText("Defeat");
            descriptionText.text = LanguageManager.GetText("Dungeon defeat");
            SetButtonsForNormalVictory();
        }

        public void ShowBossVictory()
        {
            titleText.text = LanguageManager.GetText("Boss victory");
            descriptionText.text = LanguageManager.GetText("Boss victory description");
            SetButtonsForBossVictory();
        }

        private void SetButtonsForBossVictory()
        {
            continueButton.gameObject.SetActive(false);
            goHomeButton.gameObject.SetActive(true);
        }

        private void SetButtonsForNormalVictory()
        {
            continueButton.gameObject.SetActive(true);
            goHomeButton.gameObject.SetActive(false);
        }

        public void OnContinueButtonPressed()
        {
            // 로비 씬으로 전환
            Transition.LoadLevel("1.Map", 0.5f, Color.black);
        }

        public void OnGoHomeButtonPressed()
        {
            // Map 데이터 초기화
            GameManager.GetInstance().ResetMapData();
            // 로비 씬으로 전환
            Transition.LoadLevel("0.Lobby", 0.5f, Color.black);
        }

        void DestroySelectedButton()
        {
            // 하나만 선택 가능
            Destroy(rewardButton.gameObject);
            Destroy(characterRewardButton.gameObject);
            Destroy(goldRewardButton.gameObject);
        }

        public void OnCardRewardButtonPressed()
        {
            DestroySelectedButton();
            popupCanvas.gameObject.SetActive(false);
            cardRewardEvent.Raise();
        }

        public void OnCharacterRewardButtonPressed()
        {
            DestroySelectedButton();
            popupCanvas.gameObject.SetActive(false);
            characterRewardEvent.Raise();
        }

        public void OnGoldRewardButtonPressed()
        {
            DestroySelectedButton();
            //popupCanvas.gameObject.SetActive(false);
            goldRewardEvent.Raise();
        }
    }
}
