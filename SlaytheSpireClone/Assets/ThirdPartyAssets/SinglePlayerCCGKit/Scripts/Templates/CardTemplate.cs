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
        [SerializeField]
        private string _name; // 기본 이름 (영어)
        public string Name 
        { 
            get 
            {
                // 한국어일 때는 번역된 이름 반환, 아니면 기본 이름 반환
                return LanguageManager.GetCurrentLanguage() == LanguageManager.Language.Korean ? 
                    LanguageManager.GetText(_name) : _name;
            }
            set { _name = value; }
        }
        public int Cost;
        public Material Material;
        public Sprite Picture;
        public CardType Type;
        public CardTemplate Upgrade;
        public List<Effect> Effects = new List<Effect>();
    }
}
