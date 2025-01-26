// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEditor; // Editor 기능을 사용하기 위해 필요

namespace CCGKit
{
    public class CharacterObject : MonoBehaviour
    {
        public CharacterTemplate Template;
        public RuntimeCharacter Character;

        public bool IsDead;

        public void OnCharacterDied()
        {
            if (Character.Hp.Value <= 0)
            {
                GetComponent<BoxCollider2D>().enabled = false;
                var numChilds = transform.childCount;
                for (int i = numChilds - 1; i >= 0; i--)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void OnCharacterHpChanged()
        {
            Character.Hp.SetValue(Character.Hp.Value - 10);

            if (Character.Hp.Value <= 0)
            {
                OnCharacterDied();
            }
        }


    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterObject))]
    public class CharacterObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 기본 인스펙터 내용을 그립니다
            DrawDefaultInspector();

            // CharacterManager 컴포넌트를 가져옵니다
            CharacterObject characterObject = (CharacterObject)target;

            // 버튼을 추가합니다
            if (GUILayout.Button("Hp 10 감소 테스트"))
            {
                characterObject.OnCharacterHpChanged();
            }
        }
    }
#endif
}

