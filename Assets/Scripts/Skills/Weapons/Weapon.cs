using UnityEngine;

namespace DoubleMMPrjc
{
    public class Weapon : MonoBehaviour, TimerManager.IOnCountdownEnd
    {
        [Range( 0.1f, 5f )] public float damage = 1.0f;

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
            collisionOffDelayCountdown = TimerManager.CreateCountdown( this );
        }

        public void OnTriggerEnter(Collider other)
        {
            if (attack != null && other.CompareTag( "Enemy" )) {
                other.GetComponent<Entity>().TakeDamage( attack );
            }
        }

        public void SetNextAttackInfo(Attack attack, float disableDelay = 0.3f)
        {
            this.attack = attack;
            coll.enabled = true;
            TimerManager.ResetCountdown( collisionOffDelayCountdown, disableDelay );
        }

        public void Disable(float delay = 0)
        {
            attack = null;
            if (delay == 0) {
                coll.enabled = false;
            } else {
                TimerManager.ResetCountdown( collisionOffDelayCountdown, delay );
            }
        }

        public void OnCountdownEnd(long id)
        {
            coll.enabled = false;
            attack = null;
        }
    }
}