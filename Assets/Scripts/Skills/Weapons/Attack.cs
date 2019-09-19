using System;
using UnityEngine;

namespace ColdCry
{
    public class Attack
    {
        private float damage;
        private int attackDirection;
        private Vector2 pushDirection;
        private float pushPower;
        private float pushDisableTime;
        private bool percentDamage;
        private Vector2 vector3;
        private float v;

        /// <summary>
        /// <ol>
        /// <li>cannot be blocked</li>
        /// <li>does <b>not</b> percentage damage</li>
        /// <li>doesn't push</li>
        /// </ol>
        /// </summary>
        /// <param name="damage">Damage to deal</param>
        public Attack(float damage)
            : this( damage, 0, false, Vector2.zero, 0, 0 )
        { }

        /// <summary>
        /// <ol>
        /// <li>cannot be blocked</li>
        /// <li>doesn't push</li>
        /// </ol>
        /// </summary>
        /// <param name="damage">Damage to deal</param>
        /// <param name="percentDamage"><code>TRUE</code> to deal damage as percent of hp,
        /// damage then should be from 0 to 1
        /// </param>
        public Attack(float damage, bool percentDamage)
            : this( damage, 0, percentDamage, Vector2.zero, 0, 0 )
        { }

        public Attack(float damage, int attackDirection)
            : this( damage, attackDirection, false, Vector2.zero, 0, 0 )
        { }

        public Attack(float damage, int attackDirection, bool percentDamage)
            : this( damage, attackDirection, percentDamage, Vector2.zero, 0, 0 )
        { }

        public Attack(float damage, Vector2 pushDirection, float pushPower)
            : this( damage, 0, false, pushDirection, pushPower, 0 )
        { }


        public Attack(float damage, int attackDirection, Vector2 pushDirection, float pushPower)
            : this( damage, attackDirection, false, pushDirection, pushPower, 0 )
        { }

        public Attack(float damage, int attackDirection, bool percentDamage, Vector2 pushDirection, float pushPower)
            : this( damage, attackDirection, percentDamage, pushDirection, pushPower, 0 )
        { }

        public Attack(float damage, Vector2 pushDirection, float pushPower, float pushDisableTime)
            : this( damage, 0, false, pushDirection, pushPower, pushDisableTime )
        { }

        public Attack(float damage, int attackDirection, Vector2 pushDirection, float pushPower, float pushDisableTime)
            : this( damage, attackDirection, false, pushDirection, pushPower, pushDisableTime )
        { }

        public Attack(float damage, int attackDirection, bool percentDamage, Vector2 pushDirection, float pushPower, float pushDisableTime)
        {
            if (attackDirection < -1 || attackDirection > 1)
                throw new SystemException( "Attack direction cannot be " + attackDirection );
            this.damage = damage;
            this.attackDirection = attackDirection;
            this.percentDamage = percentDamage;
            this.pushDirection = pushDirection;
            this.pushPower = pushPower;
            this.pushDisableTime = pushDisableTime;
        }

        /// <summary>
        /// Attack with zero damage
        /// </summary>
        public static Attack Zero { get => new Attack( 0 ); }
        /// <summary>
        /// Attack with one damage
        /// </summary>
        public static Attack One { get => new Attack( 1 ); }
        /// <summary>
        /// Attack that deals 100% of max HP
        /// </summary>
        public static Attack FullHP { get => new Attack( 1, true ); }
        /// <summary>
        /// Damage of attack
        /// </summary>
        public float Damage { get => damage; }
        /// <summary>
        /// Direction of attack, it's used to define if unit should block atack,
        /// 0 means that attack is unblockable
        /// </summary>
        public int AttackDirection { get => attackDirection; }
        /// <summary>
        /// Direction of push
        /// </summary>
        public Vector2 PushDirection { get => pushDirection; }
        /// <summary>
        /// Push power of attack
        /// </summary>
        public float PushPower { get => pushPower; }
        /// <summary>
        /// Push disable time of attack
        /// </summary>
        public float PushDisableTime { get => pushDisableTime; }
        /// <summary>
        /// If <code>TRUE</code> attack deals percentage damage (from 0-100) depends on <b>Damage</b> field
        /// </summary>
        public bool PercenteDamage { get => percentDamage; }
    }
}