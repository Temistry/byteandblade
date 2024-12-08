// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// This system is responsible for managing the game's turn sequence, which always follows
    /// the order:
    ///     - Player turn.
    ///     - Enemies turn.
    /// It creates event-like entities with the Event component attached to them when the turn
    /// sequence advances (beginning of player turn, end of player turn, beginning of enemies
    /// turn, end of enemies turn).
    /// </summary>
    public class TurnManagementSystem : MonoBehaviour
    {
        public GameEvent PlayerTurnBegan;
        public GameEvent PlayerTurnEnded;
        public GameEvent EnemyTurnBegan;
        public GameEvent EnemyTurnEnded;

        private bool isEnemyTurn;
        private float accTime;

        private bool isEndOfGame;

        private const float EnemyTurnDuration = 3.0f;

        protected void Update()
        {
            if (isEnemyTurn)
            {
                accTime += Time.deltaTime;
                if (accTime >= EnemyTurnDuration)
                {
                    accTime = 0.0f;
                    EndEnemyTurn();
                    BeginPlayerTurn();
                }
            }
        }

        public void BeginGame()
        {
            BeginPlayerTurn();
        }

        public void BeginPlayerTurn()
        {
            PlayerTurnBegan.Raise();
        }

        public void EndPlayerTurn()
        {
            PlayerTurnEnded.Raise();
            BeginEnemyTurn();
        }

        public void BeginEnemyTurn()
        {
            EnemyTurnBegan.Raise();
            isEnemyTurn = true;
        }

        public void EndEnemyTurn()
        {
            EnemyTurnEnded.Raise();
            isEnemyTurn = false;
        }

        public void SetEndOfGame(bool value)
        {
            isEndOfGame = value;
        }

        public bool IsEndOfGame()
        {
            return isEndOfGame;
        }
    }
}
