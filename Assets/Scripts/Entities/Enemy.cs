using DoubleMMPrjc.AI;
using UnityEngine;

namespace DoubleMMPrjc
{
    public class Enemy : NPC
    {

        public static readonly int SLEEP_CHECK_PERIOD = 50;
        public static readonly int WATCH_CHECK_PERIOD = 25;
        public static readonly int REACH_CHECK_PERIOD = 2;
        public static readonly int ATTACK_CHECK_PERIOD = 3;

        public static readonly int WATCH_POSITION_CHANGE_PERIOD = 240;
        public static readonly int WATCH_POSITION_UPDATE_PERIOD = 100;
        public static readonly int REACH_POSITION_UPDATE_PERIOD = 12;

        public static readonly float SLEEP_RANGE = 8f;
        public static readonly float WATCH_RANGE = 6f;
        public static readonly float REACH_RANGE = 1.5f;

        public static readonly float WATCH_TIME = 12f;
        public static readonly float REACH_TIME = 6f;

        private int checkPeriod = 0;
        private int watchRandomPositionChange = 0;
        private int watchFollowPositionUpdate = 0;
        private int reachFollowPositionUpdate = 0;

        private long watchCountdownId;
        private long reachCountdownId;

        private bool foundPathEalier = false;

        #region Dev Section
        private float rangeToKeepThisState = 0;
        private float rangeToNextState = SLEEP_RANGE;
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
            if (canMove) {
                watchFollowPositionUpdate++;
                if (watchFollowPositionUpdate >= WATCH_POSITION_UPDATE_PERIOD) {
                    if (currentTarget != null)
                        SetMoveDirection( currentTarget.position.x );
                    watchFollowPositionUpdate = 0;
                }
                Move( moveSpeed.current / 3f );
            }
            //TimerManager.GetRemaingCountdown( reachCountdownId, out float seconds );
            //Debug.Log( "Remain WATCH:" + seconds );
        }

        public override void ReachUpdate()
        {
            base.ReachUpdate();

            if (CheckPeriod( REACH_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (IsPlayerInRange( REACH_RANGE )) {
                    SetAttackState( "player in reach range" );
                }
            }
            if (canMove) {
                reachFollowPositionUpdate++;
                if (reachFollowPositionUpdate >= REACH_POSITION_UPDATE_PERIOD) {
                    reachFollowPositionUpdate = 0;
                    SetMoveDirection( currentTarget.position.x );
                }
                Move( moveSpeed.current );
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
        }

        public override void SetSleepState(string reason)
        {
            base.SetSleepState( reason );

            rangeToNextState = SLEEP_RANGE;
            rangeToKeepThisState = 0;

            if (dummyPositionToFollow != null) {
                Destroy( dummyPositionToFollow.gameObject );
            }          
        }

        public override void SetWatchState(string reason)
        {
            base.SetWatchState( reason );

            rangeToKeepThisState = SLEEP_RANGE;
            rangeToNextState = WATCH_RANGE;

            if (foundPathEalier) {
                TimerManager.ResetCountdown( watchCountdownId );
            }
            // FLAG RESET
            foundPathEalier = false;
            watchRandomPositionChange = 0;
            currentTarget = null;
            if (ContactArea != null) {
                SetPosToFollow( contactArea.GetRandPosInArea() );
            }
        }

        public override void SetReachState(string reason)
        {
            base.SetReachState( reason );

            rangeToKeepThisState = WATCH_RANGE;
            rangeToNextState = REACH_RANGE;

            if (dummyPositionToFollow != null) {
                Destroy( dummyPositionToFollow.gameObject );
            }
            TimerManager.ResetCountdown( reachCountdownId );
            reachFollowPositionUpdate = 0;
            
            if (!FindPath( GameManager.Character )) {
                SetWatchState( "path hasn't been found" );
            }
        }

        public override void SetAttackState(string reason)
        {
            base.SetAttackState( reason );

            rangeToKeepThisState = REACH_RANGE;
            rangeToNextState = 0;

            currentPath.Clear();
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
            reachFollowPositionUpdate = 0;
        }

        public override bool FindPath(Entity target)
        {
            if (base.FindPath( target )) {
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
            } else if (id == reachCountdownId && State == AIState.REACH) {
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

        public Transform FollowingTarget { get => currentTarget; set => currentTarget = value; }
    }
}