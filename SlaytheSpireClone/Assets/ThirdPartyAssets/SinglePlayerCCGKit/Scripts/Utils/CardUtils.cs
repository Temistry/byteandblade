// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// Miscellaneous card-related utilities.
    /// </summary>
    public static class CardUtils
    {
        public static bool CardCanBePlayed(CardTemplate card, IntVariable playerMana)
        {
            // The player can only play a card if it has a cost that is lower or
            // equal to the player's available mana.
            return card.Cost <= playerMana.Value;
        }

        public static bool CardHasTargetableEffect(CardTemplate card)
        {
            // Checks if the card contains an effect that targets a specific enemy.
            // This is used to determine whether the targeting arrow needs to be
            // enabled or not.
            foreach (var effect in card.Effects)
            {
                if (effect is TargetableEffect targetableEffect)
                {
                    if (targetableEffect.Target == EffectTargetType.TargetEnemy)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
