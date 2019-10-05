using ColdCry.AI;
using ColdCry.Core;
using ColdCry.Utility;
using UnityEngine;

namespace ColdCry.Objects
{
    public class Enemy : NPC
    {
        protected static readonly float SLEEP_RANGE = 8f;
        protected static readonly float WATCH_RANGE = 6f;
        protected static readonly float REACH_RANGE = 1.5f;

        protected static readonly float WATCH_TIME = 12f;
        protected static readonly float REACH_TIME = 15f;

        protected static readonly int SLEEP_CHECK_PERIOD = 50;
        protected static readonly int WATCH_CHECK_PERIOD = 25;
        protected static readonly int REACH_CHECK_PERIOD = 2;
        protected int checkPeriod = 0;

        protected static readonly int ATTACK_CHECK_PERIOD = 4;
        protected int attackCheckPeriod = 0;

        protected static readonly int WATCH_POSITION_CHANGE_PERIOD = 240;
        protected int watchRandomPositionChange = 0;

        protected long watchCountdownId;
        protected long reachCountdownId;

        protected bool foundPathEalier = false;

        #region Dev Section
        protected float rangeToKeepThisState = 0;
        protected float rangeToNextState = SLEEP_RANGE;
        #endregion

        #region Unity API
        public override void Start()
        {
            base.Start();
            watchCountdownId = TimerManager.Create( WATCH_TIME, this );
            reachCountdownId = TimerManager.Create( REACH_TIME, this );
        }

        public override void DrawGizmos()
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
        #endregion

        /*
        public override void SleepUpdate()
        {
            base.SleepUpdate();

            if (CheckPeriod( SLEEP_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (IsPlayerInRange( SLEEP_RANGE )) {
                    TimerManager.Reset( watchCountdownId );
                    SetWatchState( "player is in sleep range" );
                }
            }
        }

        public override void WatchUpdate()
        {
            base.WatchUpdate();

            if (CheckPeriod( WATCH_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (IsPlayerInRange( WATCH_RANGE ) && FollowTarget( GameManager.Character )) {
                    return;
                }
            }

            watchRandomPositionChange++;
            if (watchRandomPositionChange >= WATCH_POSITION_CHANGE_PERIOD) {
                watchRandomPositionChange = 0;
                if (ContactArea != null) {
                    SetPosToFollow( ContactArea.GetRandPosInArea() );
                    positionReached = false;
                }
            }

            if (!positionReached && currentTarget != null) {
                positionCheck++;
                if (positionCheck >= POSITION_CHECK_PERIOD) {
                    positionCheck = 0;
                    if (Mathf.Abs( currentTarget.position.x - transform.position.x ) <= 0.5f) {
                        positionReached = true;
                    }
                }
                Move( MoveSpeed.Current / 3f );
            }
        }

5
        {
            base.AttackUpdate();

            if (CheckPeriod( ATTACK_CHECK_PERIOD )) {
                checkPeriod = 0;
                if (!IsPlayerInRange( REACH_RANGE )) {
                    if (!FollowTarget( GameManager.Character )) {
                        SetWatchState();
                        TimerManager.Reset( refindPathCountdownId, 0.25f );
                    }
                } else {
                    Attack();
                }
            }
        }

        public override void OnAnyStateUpdate()
        {
            checkPeriod++;
            if (landed && State != AIState.ATTACK && CheckPeriod( ATTACK_CHECK_PERIOD ) && IsPlayerInRange( REACH_RANGE )) {
                SetAttackState( "player in reach range" );
                return;
            }
        }

        public override void SetSleepState(string reason = null)
        {
            base.SetSleepState( reason );

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            currentTarget = null;
            rangeToNextState = SLEEP_RANGE;
            rangeToKeepThisState = 0;

            if (dummyPositionToMove != null) {
                dummyPositionToMove.gameObject.SetActive( false );
                dummyPositionToMove = null;
            }
        }

        public override void SetWatchState(string reason = null)
        {
            base.SetWatchState( reason );

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            rangeToKeepThisState = SLEEP_RANGE;
            rangeToNextState = WATCH_RANGE;

            if (foundPathEalier) {
                TimerManager.Reset( watchCountdownId );
                watchRandomPositionChange = 0;
            }

            // FLAG RESET
            foundPathEalier = false;
            currentTarget = null;
            if (ContactArea != null) {
                SetPosToFollow( ContactArea.GetRandPosInArea() );
            }
        }

        public override void SetReachState(string reason = null)
        {
            base.SetReachState( reason );

            // Sets ranges for gizmos
            rangeToKeepThisState = WATCH_RANGE;
            rangeToNextState = REACH_RANGE;

            // Resets timer to count time for reach state to end
            if (TimerManager.HasEnded( reachCountdownId )) {
                TimerManager.Reset( reachCountdownId );
            }
            TimerManager.Stop( refindPathCountdownId );
        }

        // TODO
        // DODAĆ NOWY STATE CZEKAJĄCY Z SZUKANIEM ŚCIEŻKI

        public override void SetFollowState(string reason = null)
        {
            base.SetFollowState( reason );

            // Sets ranges for gizmos
            rangeToKeepThisState = WATCH_RANGE;
            rangeToNextState = REACH_RANGE;

            TimerManager.Reset( reachCountdownId );
        }

        public override void SetAttackState(string reason = null)
        {
            base.SetAttackState( reason );

            /*
            if (followedEntity) {
                currentTarget = followedEntity.transform;
                followedEntity.RemoveFollower( this );
            }

            rangeToKeepThisState = REACH_RANGE;
            rangeToNextState = 0;
        }

        protected override void OnAnyStateChange(string reason)
        {
            base.OnAnyStateChange( reason );
            checkPeriod = 0;
            positionCheck = 0;
        }
        */

        public override void OnDie()
        {
            base.Die();
            //TimerManager.Destroy( refindPathCountdownId );
            TimerManager.Destroy( watchCountdownId );
            TimerManager.Destroy( reachCountdownId );
            rangeToKeepThisState = 0;
            rangeToNextState = 0;
        }

        public override void ResetUnit()
        {
            base.ResetUnit();
          //  TimerManager.Destroy( refindPathCountdownId );
            TimerManager.Destroy( watchCountdownId );
            TimerManager.Destroy( reachCountdownId );
            watchCountdownId = TimerManager.Create( WATCH_TIME, this );
            reachCountdownId = TimerManager.Create( REACH_TIME, this );
          //  refindPathCountdownId = TimerManager.Create( this );
        //    SetSleepState( "unit has been reseted" );
            checkPeriod = 0;
            watchRandomPositionChange = 0;
         //   reachDirectionUpdate = 0;
        }

     /*   public override bool FollowTarget(Entity target)
        {
            if (base.FollowTarget( target )) {
                foundPathEalier = true;
                return true;
            }
            return false;
        }*/

        /// <summary>
        /// Metoda w której AI powinno decydować o sposobie ataku
        /// </summary>
        public virtual void Attack()
        {
            //  Debug.Log( name + " PERFORMS ATTACK!" );
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

        public override void OnCountdownEnd(long id, float overtime)
        {
            base.OnCountdownEnd( id, overtime );
            /*    if (id == refindPathCountdownId) {
                    if (!FollowTarget( followedEntity )) {
                        TimerManager.Reset( refindPathCountdownId );
                    }
                } else if (id == watchCountdownId && State == AIState.WATCH) {
                    if (IsPlayerInRange( SLEEP_RANGE )) {
                        TimerManager.Reset( watchCountdownId );
                    } else {
                        TimerManager.Stop( refindPathCountdownId );
                        SetSleepState( "player wasn't in sleep range while enemy had WATCH state when timer has ended" );
                    }
                } else if (id == reachCountdownId && ( State == AIState.REACH || State == AIState.FOLLOW )) {
                    if (IsPlayerInRange( WATCH_RANGE )) {
                        TimerManager.Reset( reachCountdownId );
                    } else {
                        TimerManager.Stop( refindPathCountdownId );
                        SetWatchState( "player wasn't in sleep range while enemy had REACH state when timer has ended" );
                    }
                }*/
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

        public override void OnContactAreaEnter(ContactArea contactArea)
        {
            // Checking in follow state when entity (propably) fall into wrong contact
            // area by moving or player push
          /*  if (State == AIState.FOLLOW && !( contactArea == followedEntity.ContactArea )) {
                if (followedEntity.ContactArea == null) {
                    Debug.Log( "Starting interval refinding(FOLLOW)..." );
                    StartPathRefind( 0.7f );
                } else if (!FollowTarget( followedEntity )) {
                    SetWatchState( "entity had FOLLOW state, enters new area that doesn't\n" +
                                   "contains node , propably fallen on wrong area after jump,\n" +
                                   "had to find new path but cannot find one to reach target" );
                }
            }
            // Checking in reach state when entity (propably) fall into wrong contact area
            else if (State == AIState.REACH && HasPath && !contactArea.Contains( currentCn.Node )) {
                if (followedEntity.ContactArea == null) {
                    StartPathRefind( 0.7f );
                } else if (!FollowTarget( entityToFollowAfterPath )) {
                    SetWatchState( "entity had REACH state, enters new area that doesn't\n" +
                                   "contains node , propably fallen on wrong area after jump,\n" +
                                   "had to find new path but cannot find one to reach target" );
                }

            }*/
        }

        public override void OnContactAreaExit(ContactArea contactArea)
        {

        }
    }
}