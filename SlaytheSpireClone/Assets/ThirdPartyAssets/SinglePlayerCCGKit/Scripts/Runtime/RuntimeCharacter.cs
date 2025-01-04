// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    public class RuntimeCharacter
    {
        public IntVariable Hp;
        public IntVariable Shield;

        public IntVariable Mana;

        public StatusVariable Status;

        public int MaxHp;

        public CardTemplate CurrentUsedCard;    // 현재 사용한 카드
    }
}
