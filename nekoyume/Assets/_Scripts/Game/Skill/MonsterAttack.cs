using System;
using Nekoyume.Data.Table;
using Nekoyume.Game.CC;
using Nekoyume.Game.Character;

namespace Nekoyume.Game.Skill
{
    public class MonsterAttack : SkillBase
    {
        private void Awake()
        {
            _targetTag = Tag.Player;
        }

        protected override bool _Use()
        {
            float range = (float)Data.Range / (float)Game.PixelPerUnit;

            var objectPool = GetComponentInParent<Util.ObjectPool>();
            var damager = objectPool.Get<Trigger.Damager>();
            Damager(damager, -range, "hit_04");

            return true;
        }

        protected override void OnDamage(CharacterBase character)
        {
            base.OnDamage(character);
            switch (new Random().Next(1, 21))
            {
                case 1:
                    var dotDamager = character.gameObject.AddComponent<DotDamager>();
                    dotDamager.Set(attackType: AttackType.Middle, damagePerTick: 4, duration: 3.0f);
                    break;
                case 2:
                    var silence = character.gameObject.AddComponent<Silence>();
                    silence.Set(duration: 5.0f);
                    break;
                case 3:
                    var stun = character.gameObject.AddComponent<Stun>();
                    stun.Set(duration: 2.0f);
                    break;
                case 4:
                    var airborne = character.gameObject.AddComponent<Airborne>();
                    airborne.Set(duration: 2.0f);
                    break;
                case 5:
                    var slow = character.gameObject.AddComponent<Slow>();
                    slow.Set(attackSpeedMultiplier: 2.0f, walkSpeedMultiplier: 0.1f, duration: 3.0f);
                    break;
            }
        }
    }
}
