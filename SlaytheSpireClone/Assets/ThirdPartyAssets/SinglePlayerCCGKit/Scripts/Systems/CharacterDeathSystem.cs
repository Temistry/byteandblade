// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This system handles the death of a character to end the battle and display the end-of-battle
    /// popup on the screen.
    /// </summary>
    public class CharacterDeathSystem : BaseSystem
    {
#pragma warning disable 649
        [SerializeField]
        private float endBattlePopupDelay = 1.0f;
#pragma warning restore 649
        
        public void OnPlayerHpChanged(int hp)
        {
            if (hp <= 0)
            {
                EndGame(true);
            }
        }

        public void OnEnemy1HpChanged(int hp)
        {
            if (hp <= 0)
            {
                Enemies[0].IsDead = true;
                OnEnemyDied();
                if (IsEndOfGame())
                {
                    EndGame(false);
                }
            }
        }

        public void OnEnemy2HpChanged(int hp)
        {
            if (hp <= 0)
            {
                Enemies[1].IsDead = true;
                OnEnemyDied();
                if (IsEndOfGame())
                {
                    EndGame(false);
                }
            }
        }

        private void OnEnemyDied()
        {
            foreach (var enemy in Enemies)
            {
                enemy.OnCharacterDied();
            }
        }

        private bool IsEndOfGame()
        {
            foreach (var enemy in Enemies)
            {
                if (!enemy.IsDead)
                    return false;
            }

            return true;
        }

        public void EndGame(bool playerDied)
        {
            StartCoroutine(ShowEndBattlePopup(playerDied));
        }

        private IEnumerator ShowEndBattlePopup(bool playerDied)
        {
            yield return new WaitForSeconds(endBattlePopupDelay);

            var popupOverlay = FindFirstObjectByType<PopupOverlay>();
            var endBattlePopup = FindFirstObjectByType<EndBattlePopup>();
            if (popupOverlay != null && endBattlePopup != null)
            {
                popupOverlay.Show();
                endBattlePopup.Show();

                var gameInfo = FindFirstObjectByType<GameInfo>();

                if (playerDied)
                {
                    endBattlePopup.SetDefeatText();
                    gameInfo.PlayerWonEncounter = false;
                }
                else
                {
                    endBattlePopup.SetVictoryText();
                    gameInfo.PlayerWonEncounter = true;
                }

                var turnManagementSystem = FindFirstObjectByType<TurnManagementSystem>();
                turnManagementSystem.SetEndOfGame(true);
            }
        }
    }
}
