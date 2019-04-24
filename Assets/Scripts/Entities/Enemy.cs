using DoubleMMPrjc.AI;
using UnityEngine;

namespace DoubleMMPrjc
{
    public class Enemy : NPC
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

        public static readonly float SLEEP_RANGE = 12f;
        public static readonly float WATCH_RANGE = 7f;
        public static readonly float CHASE_RANGE = 4f;

        public static readonly float ATTACK_RANGE = 1.5f;

        public static readonly float WATCH_TIME = 6.5f;
        public static readonly float CHASE_TIME = 13f;

        [SerializeField] private AIState state = AIState.SLEEP;

        private int checkPeriod = 0;
        private int watchRandomPositionChange = 0;
        private int chaseFollowPositionUpdate = 0;

        private long watchCountdownId;
        private long chaseCountdownId;

        private bool foundPathEalier = false;

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
                            UpdateMoveDirection( new Vector2( Random.Range( -12f, 24.7f ), transform.position.y ) );
                        }
                        if (canMove) {
                            Move( moveSpeed.current / 2f );
                        }
                        break;

                    case AIState.CHASE:
                        if (CheckPeriod( CHASE_CHECK_PERIOD )) {
                            checkPeriod = 0;
                            if (IsPlayerInRange( CHASE_RANGE )) {
                                State = AIState.ATTACK;
                            }
                        }
                        /*chaseFollowPositionUpdate++;
                        if (canMove) {
                            if (chaseFollowPositionUpdate >= CHASE_POSITION_UPDATE_PERIOD) {
                                chaseFollowPositionUpdate = 0;
                                UpdateMovePosition( followTarget.position );
                            }
                            transform.Translate( moveDirection * moveSpeed.current * Time.fixedDeltaTime, Space.World );
                        }*/
                        if (canMove) {
                            chaseFollowPositionUpdate++;
                            if (!HasPath() && chaseFollowPositionUpdate >= CHASE_POSITION_UPDATE_PERIOD) {
                                chaseFollowPositionUpdate = 0;
                                followingTarget = GameManager.Character.transform;
                                UpdateMoveDirection( followingTarget.position );
                            }
                            Move( moveSpeed.current / 5f );
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

        public override void FindPath(Entity target)
        {
            base.FindPath( target );
            if (HasPath()) {
                NextNodeInPath();
                foundPathEalier = true;
            } else {
                State = AIState.WATCH;
            }
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
            if (currentNode != null) {
                Gizmos.DrawLine( transform.position, currentNode.transform.position );
            }
        }

        /// <summary>
        /// State of enemy's AI
        /// </summary>
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
                        currentRange = WATCH_RANGE;
                        if (foundPathEalier) {
                            TimerManager.ResetCountdown( watchCountdownId );
                        }
                        foundPathEalier = false;
                        UpdateMoveDirection( new Vector2( Random.Range( -12f, 24.7f ), transform.position.y ) );
                        watchRandomPositionChange = 0;
                        ResetPath();
                        break;
                    case AIState.CHASE:
                        TimerManager.ResetCountdown( chaseCountdownId );
                        chaseFollowPositionUpdate = 0;
                        currentRange = CHASE_RANGE;
                        FindPath(GameManager.Character);
                        break;
                    case AIState.ATTACK:
                        currentRange = ATTACK_RANGE;
                        ResetPath();
                        break;
                }
            }
        }
        public Transform FollowingTarget { get => followingTarget; set => followingTarget = value; }
    }
}