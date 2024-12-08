// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCGKit
{
    [Serializable]
    [CreateAssetMenu(
        menuName = "Single-Player CCG Kit/Templates/Card",
        fileName = "Card",
        order = 0)]
    public class CardTemplate : ScriptableObject
    {
        public int Id;
        public string Name;
        public int Cost;
        public Material Material;
        public Sprite Picture;
        public CardType Type;
        public CardTemplate Upgrade;
        public List<Effect> Effects = new List<Effect>();
    }
}
