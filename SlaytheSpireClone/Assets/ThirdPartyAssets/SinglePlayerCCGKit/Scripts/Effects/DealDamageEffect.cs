// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// The type corresponding to the "Deal X damage" card effect.
    /// </summary>
    [CreateAssetMenu(fileName = "New Deal Damage Effect", menuName = "Effects/Deal Damage Effect")]
    public class DealDamageEffect : IntegerEffect, IEntityEffect
    {
        public override string GetName()
        {
            return $"Deal {Value.ToString()} damage";
        }

        /// <summary>
        /// 대상에게 피해를 입히는 효과를 처리합니다.
        /// </summary>
        /// <param name="instigator">피해를 입히는 캐릭터</param>
        /// <param name="target">피해를 받는 캐릭터</param>
        public override void Resolve(RuntimeCharacter instigator, RuntimeCharacter target)
        {
            // 대상의 현재 HP와 Shield 값을 가져옵니다.
            var targetHp = target.Hp;
            var targetShield = target.Shield;
            var hp = targetHp.Value;
            var shield = targetShield.Value;
            // 이 효과가 입히는 기본 피해량을 가져옵니다.
            var damage = Value;

            // 공격자의 "강력" 상태 효과 값을 추가합니다.
            var strength = instigator.Status.GetValue("Strength");
            damage += strength;

            // 공격자의 "약점" 상태 효과가 있으면 피해량을 조정합니다.
            var weak = instigator.Status.GetValue("Weak");
            if (weak > 0)
                damage = (int)Mathf.Floor(damage * 0.75f);

            // 피해량이 Shield보다 크면 Shield를 먼저 소모하고 남은 피해량으로 HP를 감소시킵니다.
            if (damage >= shield)
            {
                var newHp = hp - (damage - shield);
                if (newHp < 0)
                    newHp = 0;
                targetHp.SetValue(newHp);
                targetShield.SetValue(0);
            }
            // 피해량이 Shield보다 작은 경우 Shield만 감소시킵니다.
            else
            {
                targetShield.SetValue(shield - damage);
            }

            // 피해 효과음 재생
            SoundEffectHelper.PlayAttackSound(damage);
        }
    }
}
