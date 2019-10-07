using ColdCry.Core;
using ColdCry.Objects;
using ColdCry.Utility;
using UnityEngine;

namespace ColdCry
{
    public class Weapon : MonoBehaviour, IOnCountdownEnd
    {
        private Character character;
        private Attack attack;
        private Collider coll;
        private long collisionOffDelayCountdown;

        public void Awake()
        {
            coll = GetComponent<Collider>();
        }

        public void Start()
        {
            character = (Character) GameManager.GetEntityByName( "Player" );
            collisionOffDelayCountdown = TimerManager.Create( this );
        }

        public void OnTriggerEnter(Collider other)
        {
            if (attack != null && other.CompareTag( "Enemy" )) {
                other.GetComponent<Entity>().TakeDamage( attack );
            }
        }

        /// <summary>
        /// Sets what weapon will do on collision with game objects
        /// </summary>
        /// <param name="attack">Attack with information about collide behaviour</param>
        /// <param name="disableDelay">Amount of time to disable weapon collider</param>
        public void SetNextAttackInfo(Attack attack, float disableDelay = 0.3f)
        {
            this.attack = attack;
            coll.enabled = true;
            TimerManager.Restart( collisionOffDelayCountdown, disableDelay );
        }

        public void Disable(float delay = 0)
        {
            attack = null;
            if (delay == 0) {
                coll.enabled = false;
            } else {
                TimerManager.Restart( collisionOffDelayCountdown, delay );
            }
        }

        public void OnCountdownEnd(long id, float overtime)
        {
            coll.enabled = false;
            attack = null;
        }
    }
}