using DoubleMMPrjc.AI;
using UnityEngine;

namespace DoubleMMPrjc
{
    public class Enemy : NPC
    {
        protected static readonly float SLEEP_RANGE = 8f;
        protected static readonly float WATCH_RANGE = 6f;
        protected static readonly float REACH_RANGE = 1.5f;

        protected static readonly float WATCH_TIME = 12f;
        protected static readonly float REACH_TIME = 6f;

        protected static readonly int SLEEP_CHECK_PERIOD = 50;
        protected static readonly int WATCH_CHECK_PERIOD = 25;
        protected static readonly int REACH_CHECK_PERIOD = 2;
        protected int checkPeriod = 0;

        protected static readonly int ATTACK_CHECK_PERIOD = 3;
        protected int attackCheckPeriod = 0;

        protected static readonly int WATCH_POSITION_CHANGE_PERIOD = 240;
        protected int watchRandomPositionChange = 0;

        protected static readonly int WATCH_POSITION_UPDATE_PERIOD = 100;
        protected int watchFollowPositionUpdate = 0;

        protected long watchCountdownId;
        protected long reachCountdownId;

        protected bool foundPathEalier = false;

        #region Dev Section
        protected float rangeToKeepThisState = 0;
        protected float rangeToNextState = SLEEP_RANGE;
        #endregion

        public override void Start()
        {
            base.Start();
            watchCountdownId = TimerManager.CreateCountdown( WATCH_TIME, this );
            reachCountdownId = TimerManager.CreateCountdown( REACH_TIME, this );
        }

        public override void SleepUpdate()
        {
            base.SleepUpdate();

            if (CheckPeriod( SLEEP_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (IsPlayerInRange( SLEEP_RANGE )) {
                    TimerManager.ResetCountdown( watchCountdownId );
                    SetWatchState( "player is in sleep range" );
                }
            }
        }

        public override void WatchUpdate()
        {
            base.WatchUpdate();

            if (CheckPeriod( WATCH_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (IsPlayerInRange( WATCH_RANGE )) {
                    SetReachState( "player is in watch range" );
                }
            }

            watchRandomPositionChange++;
            if (watchRandomPositionChange >= WATCH_POSITION_CHANGE_PERIOD) {
                watchRandomPositionChange = 0;
                if (ContactArea != null) {
                    SetPosToFollow( contactArea.GetRandPosInArea() );
                    watchFollowPositionUpdate = 0;
                }
            }

            watchFollowPositionUpdate++;
            if (watchFollowPositionUpdate >= WATCH_POSITION_UPDATE_PERIOD) {
                if (currentTarget != null)
                    SetMoveDirection( currentTarget.position.x );
                watchFollowPositionUpdate = 0;
            }

            Move( moveSpeed.current / 3f );
        }

        public override void ReachUpdate()
        {
            base.ReachUpdate();

            if (canMove) {
                reachDirectionUpdate++;
                if (reachDirectionUpdate >= REACH_DIRECTION_UPDATE_PERIOD) {
                    reachDirectionUpdate = 0;
                    SetMoveDirection( currentTarget.position.x );
                }
                Move( moveSpeed.current );
            }
        }

        public override void FollowUpdate()
        {
            base.FollowUpdate();

            if (CheckPeriod( REACH_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (IsPlayerInRange( REACH_RANGE )) {
                    SetAttackState( "player in reach range" );
                }
            }

        }

        public override void AttackUpdate()
        {
            base.AttackUpdate();

            if (CheckPeriod( ATTACK_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (!IsPlayerInRange( REACH_RANGE )) {
                    SetReachState( "player is out of attack range" );
                } else {
                    Attack();
                }
            }
        }

        public override void OnAnyStateUpdate()
        {
            checkPeriod++;
            /* if (CheckPeriod( REACH_CHECK_PERIOD )) {
                 checkPeriod = 0;
                 if (IsPlayerInRange( REACH_RANGE )) {
                     SetAttackState( "player in reach range" );
                 }
             }*/
        }

        public override void SetSleepState(string reason)
        {
            base.SetSleepState( reason );

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            rangeToNextState = SLEEP_RANGE;
            rangeToKeepThisState = 0;

            if (dummyPositionToMove != null) {
                dummyPositionToMove.gameObject.SetActive( false );
                dummyPositionToMove = null;
            }
        }

        public override void SetWatchState(string reason)
        {
            base.SetWatchState( reason );

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            rangeToKeepThisState = SLEEP_RANGE;
            rangeToNextState = WATCH_RANGE;

            if (foundPathEalier) {
                TimerManager.ResetCountdown( watchCountdownId );
                watchRandomPositionChange = 0;
            }

            // FLAG RESET
            foundPathEalier = false;
            currentTarget = null;
            if (ContactArea != null) {
                SetPosToFollow( contactArea.GetRandPosInArea() );
            }
        }

        public override void SetReachState(string reason)
        {
            base.SetReachState( reason );

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            // Sets ranges for gizmos
            rangeToKeepThisState = WATCH_RANGE;
            rangeToNextState = REACH_RANGE;

            // Resets timer to count time for reach state to end
            TimerManager.ResetCountdown( reachCountdownId );

            // If cannot find path to given entity then backs to watch state
            if (!FollowTarget( GameManager.Character )) {
                SetWatchState( "cannot find path to target" );
            }
        }

        public override void SetFollowState(string reason)
        {
            base.SetFollowState( reason );

            TimerManager.ResetCountdown( reachCountdownId );
        }

        public override void SetAttackState(string reason)
        {
            base.SetAttackState( reason );

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            rangeToKeepThisState = REACH_RANGE;
            rangeToNextState = 0;
        }

        protected override void OnAnyStateChange(string reason)
        {
            base.OnAnyStateChange( reason );
            checkPeriod = 0;
        }

        public override void Die()
        {
            base.Die();
            TimerManager.RemoveCountdown( watchCountdownId );
            TimerManager.RemoveCountdown( reachCountdownId );
        }

        public override void ResetUnit()
        {
            base.ResetUnit();
            TimerManager.RemoveCountdown( watchCountdownId );
            TimerManager.RemoveCountdown( reachCountdownId );
            watchCountdownId = TimerManager.CreateCountdown( WATCH_TIME, this );
            reachCountdownId = TimerManager.CreateCountdown( REACH_TIME, this );
            SetSleepState( "unit has been reseted" );
            checkPeriod = 0;
            watchRandomPositionChange = 0;
            reachDirectionUpdate = 0;
        }

        public override bool FollowTarget(Entity target)
        {
            if (base.FollowTarget( target )) {
                foundPathEalier = true;
                return true;
            }
            return false;
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

        public override void OnCountdownEnd(long id)
        {
            base.OnCountdownEnd( id );
            if (id == watchCountdownId && State == AIState.WATCH) {
                if (IsPlayerInRange( SLEEP_RANGE )) {
                    TimerManager.ResetCountdown( watchCountdownId );
                } else {
                    SetSleepState( "player wasn't in sleep range while enemy had WATCH state when timer has ended" );
                }
            } else if (id == reachCountdownId && ( State == AIState.REACH || State == AIState.FOLLOW )) {
                if (IsPlayerInRange( WATCH_RANGE )) {
                    TimerManager.ResetCountdown( reachCountdownId );
                } else {
                    SetWatchState( "player wasn't in sleep range while enemy had REACH state when timer has ended" );
                }
            }
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
            if (GameManager.DrawEnemyRange) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere( transform.position, rangeToNextState );
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere( transform.position, rangeToKeepThisState );
            }
            if (GameManager.DrawAIDestination) {
                Gizmos.color = Color.blue;
                if (currentTarget != null) {
                    Gizmos.DrawLine( transform.position, currentTarget.position );
                }
            }
        }

        public override void OnContactAreaEnter(ContactArea contactArea)
        {

        }

        public override void OnContactAreaExit(ContactArea contactArea)
        {

        }

        public Transform FollowingTarget { get => currentTarget; set => currentTarget = value; }
    }
}