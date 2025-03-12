// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The type corresponding to the "Clone X card" card effect.
    /// (한국어 번역) "X 카드 복제" 카드 효과에 해당하는 유형입니다. 카드를 복제해서 덱 안에 넣습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Clone Card Effect", menuName = "Effects/Clone Card Effect")]
    public class CloneCardEffect : IntegerEffect, IEntityEffect
    {
        public override string GetName()
        {
            return $"Clone {Value.ToString()} card";
        }

        public override void Resolve(RuntimeCharacter instigator, RuntimeCharacter target)
        {
            DeckDrawingSystem deckDrawingSystem = FindFirstObjectByType<DeckDrawingSystem>();
            deckDrawingSystem.AddCardToDeck(instigator.CurrentUsedCard);
        }
    }
}
