using UnityEngine;

namespace DoubleMMPrjc
{
    public class Enemy : Entity
    {
        public enum State
        {
            SLEEP, WATCH, CHASE, ATTACK
        }

        public static readonly int SLEEP_CHECK_PERIOD = 10;
        public static readonly int WATCH_CHECK_PERIOD = 5;
        public static readonly int CHASE_CHECK_PERIOD = 2;
        public static readonly int ATTACK_CHECK_PERIOD = 1;

        public static readonly float SLEEP_RANGE = 3f;

        public State state = State.WATCH;
        private int checkPeriod = 0;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!( isDead || isPaused )) {
                checkPeriod++;
                switch (state) {
                    case State.SLEEP:
                        if (CanCheck( SLEEP_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( SLEEP_RANGE )) {
                                state = State.WATCH;
                                Debug.Log( "Changed to : " + state );
                            }
                        }
                        break;
                    case State.WATCH:
                        if (CanCheck( WATCH_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( SLEEP_RANGE )) {
                                state = State.CHASE;
                                Debug.Log( "Changed to : " + state );
                            }
                        }
                        break;
                    case State.CHASE:
                        if (CanCheck( CHASE_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( SLEEP_RANGE )) {
                                state = State.ATTACK;
                                Debug.Log( "Changed to : " + state );
                            }
                        }
                        break;
                    case State.ATTACK:
                        if (CanCheck( ATTACK_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (!IsPlayerInRange( SLEEP_RANGE )) {
                                state = State.CHASE;
                                Debug.Log( "Changed to : " + state );
                            } else {
                                Attack();
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Metoda w której AI powinno decydować o zachowaniu i sposobie ruchu
        /// </summary>
        public virtual void Movement(Vector2 position)
        {

        }

        /// <summary>
        /// Metoda w której AI powinno decydować o sposobie ataku
        /// </summary>
        public virtual void Attack()
        {
            Debug.Log( name + " PERFORMS ATTACK!" );
        }

        /// <summary>
        /// Checks if player is in unit range
        /// </summary>
        /// <param name="range">Range is the radius to check if player is within</param>
        /// <returns><code>TRUE</code> if player is in range, otherwise <code>FALSE</code></returns>
        public virtual bool IsPlayerInRange(float range)
        {
            return Vector2.Distance( GameManager.Character.transform.position, transform.position ) <= range;
        }

        /// <summary>
        /// Checks if unit should look for player
        /// </summary>
        /// <param name="period">Period to be exceeded</param>
        /// <returns><code>TRUE</code> if <b>checkPeriod</b> field is equal or larger than <b>period</b> parameter</returns>
        protected virtual bool CanCheck(int period)
        {
            return checkPeriod >= period;
        }
    }
}