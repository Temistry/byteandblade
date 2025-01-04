// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace CCGKit
{
    /// <summary>
    /// The base class of all the targetable card effects that have an associated integer value
    /// (e.g., "Deal X damage", "Gain X HP").
    /// (한국어 번역) 모든 대상이 할당된 정수 값을 가진 카드 효과의 기본 클래스입니다.
    /// (예: "X 데미지 주기", "X HP 얻기").
    /// </summary>
    public abstract class IntegerEffect : TargetableEffect
    {
        public int Value;

#if UNITY_EDITOR
        public override void Draw(Rect rect)
        {
            var popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.fixedWidth = 100;
            var numberFieldStyle = new GUIStyle(EditorStyles.numberField);
            numberFieldStyle.fixedWidth = 40;

            Target = (EffectTargetType)EditorGUI.EnumPopup(rect, "Target", Target, popupStyle);
            rect.y += RowHeight;

            Value = EditorGUI.IntField(rect, "Value", Value, numberFieldStyle);
        }

        public override int GetNumRows()
        {
            return 3;
        }
#endif
    }
}
