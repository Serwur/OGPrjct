using UnityEngine;

namespace DoubleMMPrjc
{
    public class Enemy : Entity
    {
        public enum AIState
        {
            SLEEP, WATCH, CHASE, ATTACK
        }

        public static readonly int SLEEP_CHECK_PERIOD = 50;
        public static readonly int WATCH_CHECK_PERIOD = 25;
        public static readonly int CHASE_CHECK_PERIOD = 2;
        public static readonly int ATTACK_CHECK_PERIOD = 3;

        public static readonly int WATCH_POSITION_CHANGE_PERIOD = 240;
        public static readonly int CHASE_POSITION_UPDATE_PERIOD = 12;

        public static readonly float SLEEP_RANGE = 4.5f;
        public static readonly float WATCH_RANGE = 2.5f;
        public static readonly float CHASE_RANGE = 1.6f;
        public static readonly float ATTACK_RANGE = 1.5f;

        public static readonly float WATCH_TIME = 6.5f;
        public static readonly float CHASE_TIME = 5.0f;

        private AIState state = AIState.SLEEP;

        private int checkPeriod = 0;
        private int watchRandomPositionChange = 0;
        private int chaseFollowPositionUpdate = 0;

        private Vector2 currentTarget;
        private Vector2 moveDirection;
        private Transform followTarget;

        private long watchCountdownId;
        private long chaseCountdownId;

        #region Dev Section
        private float currentRange = SLEEP_RANGE;
        #endregion

        public override void Start()
        {
            base.Start();
            watchCountdownId = TimerManager.CreateCountdown( WATCH_TIME, this );
            chaseCountdownId = TimerManager.CreateCountdown( CHASE_TIME, this );
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!( isDead || isPaused )) {
                checkPeriod++;
                switch (state) {
                    case AIState.SLEEP:
                        if (CheckPeriod( SLEEP_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( SLEEP_RANGE )) {
                                State = AIState.WATCH;
                            }
                        }
                        break;

                    case AIState.WATCH:
                        if (CheckPeriod( WATCH_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( WATCH_RANGE )) {
                                State = AIState.CHASE;
                                break;
                            }
                        }
                        watchRandomPositionChange++;
                        if (watchRandomPositionChange >= WATCH_POSITION_CHANGE_PERIOD) {
                            watchRandomPositionChange = 0;
                            UpdateMovePosition( new Vector2( Random.Range( -12f, 24.7f ), transform.position.y ) );
                        }
                        if (canMove)
                            transform.Translate( moveDirection * moveSpeed.current / 2f * Time.fixedDeltaTime, Space.World );
                        break;

                    case AIState.CHASE:
                        if (CheckPeriod( CHASE_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( CHASE_RANGE )) {
                                State = AIState.ATTACK;
                            }
                        }
                        chaseFollowPositionUpdate++;
                        if (canMove) {
                            if (chaseFollowPositionUpdate >= CHASE_POSITION_UPDATE_PERIOD) {
                                chaseFollowPositionUpdate = 0;
                                UpdateMovePosition( followTarget.position );
                            }
                            transform.Translate( moveDirection * moveSpeed.current * Time.fixedDeltaTime, Space.World );
                        }
                        break;

                    case AIState.ATTACK:
                        if (CheckPeriod( ATTACK_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (!IsPlayerInRange( ATTACK_RANGE )) {
                                State = AIState.CHASE;
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
        public virtual void UpdateMovePosition(Vector2 position)
        {
            currentTarget = position;
            moveDirection = new Vector2( currentTarget.x - transform.position.x, 0 ).normalized;
        }

        public override void OnCountdownEnd(long id)
        {
            base.OnCountdownEnd( id );
            if (id == watchCountdownId && state == AIState.WATCH) {
                if (IsPlayerInRange( SLEEP_RANGE )) {
                    TimerManager.ResetCountdown( watchCountdownId );
                } else {
                    State = AIState.SLEEP;
                }
            } else if (id == chaseCountdownId && state == AIState.CHASE) {
                if (IsPlayerInRange( WATCH_RANGE )) {
                    TimerManager.ResetCountdown( chaseCountdownId );
                } else {
                    State = AIState.WATCH;
                }
            }
        }

        public override void Die()
        {
            base.Die();
            TimerManager.RemoveCountdown( watchCountdownId );
            TimerManager.RemoveCountdown( chaseCountdownId );
        }

        public override void ResetUnit()
        {
            base.ResetUnit();
            watchCountdownId = TimerManager.CreateCountdown( WATCH_TIME, this );
            chaseCountdownId = TimerManager.CreateCountdown( CHASE_TIME, this );
            State = AIState.SLEEP;
            checkPeriod = 0;
            watchRandomPositionChange = 0;
            chaseFollowPositionUpdate = 0;
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
        protected virtual bool CheckPeriod(int period)
        {
            return checkPeriod >= period;
        }


        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( transform.position, currentRange );
        }

        public AIState State
        {
            get => state;

            private set {
                state = value;
                Debug.Log( "Changed to : " + state );
                checkPeriod = 0;
                switch (state) {
                    case AIState.SLEEP:
                        currentRange = SLEEP_RANGE;
                        break;
                    case AIState.WATCH:
                        UpdateMovePosition( new Vector2( Random.Range( -12f, 24.7f ), transform.position.y ) );
                        TimerManager.ResetCountdown( watchCountdownId );
                        watchRandomPositionChange = 0;
                        currentRange = WATCH_RANGE;
                        break;
                    case AIState.CHASE:
                        TimerManager.ResetCountdown( chaseCountdownId );
                        chaseFollowPositionUpdate = 0;
                        followTarget = GameManager.Character.transform;
                        UpdateMovePosition( followTarget.position );
                        currentRange = CHASE_RANGE;
                        break;
                    case AIState.ATTACK:
                        currentRange = ATTACK_RANGE;
                        break;
                }
            }
        }

        public Transform FollowTarget { get => followTarget; set => followTarget = value; }
    }
}