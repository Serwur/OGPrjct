using UnityEngine;

namespace DoubleMMPrjc
{
    public class Attack
    {
        private float damage;
        private Vector3 direction;
        private float pushPower;
        private float pushDisableTime;
        private bool percentDamage;

        public Attack(float damage) : this( damage, Vector3.zero )
        { }

        public Attack(float damage, Vector3 direction) : this( damage, direction, 0 )
        { }

        public Attack(float damage, Vector3 direction, float pushPower) : this( damage, direction, pushPower, 0 )
        { }

        public Attack(float damage, Vector3 direction, float pushPower, float pushDisableTime) : this( damage, direction, pushPower, pushDisableTime, false )
        { }

        public Attack(float damage, bool percentDamage) : this( damage, Vector3.zero, 0, 0, percentDamage )
        { }

        public Attack(float damage, Vector3 direction, float pushPower, float pushDisableTime, bool percentDamage)
        {
            this.damage = damage;
            this.direction = direction;
            this.pushPower = pushPower;
            this.pushDisableTime = pushDisableTime;
            this.percentDamage = percentDamage;
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
        /// Damage of attack
        /// </summary>
        public float Damage { get => damage; }
        /// <summary>
        /// Direction of attack
        /// </summary>
        public Vector3 Direction { get => direction; }
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