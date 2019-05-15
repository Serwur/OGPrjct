using DoubleMMPrjc.AI;
using UnityEngine;

namespace DoubleMMPrjc
{
    public abstract class NPC : Entity
    {
        protected AIPathList currentPath = new AIPathList();

        protected Vector2 moveDirection;

        /// <summary>
        /// Used to keep reference to which entity should move when path will be ended
        /// </summary>
        protected Entity entityToFollowAfterPath;
        /// <summary>
        /// Used for following state to keep reference of followed entity
        /// </summary>
        protected Entity followedEntity;
        protected Transform currentTarget;

        protected Dummy dummyPositionToMove;
        protected ComplexNode currentCn;

        protected static readonly int FOLLOW_DIRECTION_UPDATE_PERIOD = 8;
        protected int followDirectionUpdate = 0;

        protected static readonly int REACH_DIRECTION_UPDATE_PERIOD = 12;
        protected int reachDirectionUpdate = 0;

        [SerializeField] private AIState state = AIState.SLEEP;

        #region DEV SECTION
        private static readonly string UNDEFINED = "UNDEFINED";
        protected string stateReasonChange = UNDEFINED;
        #endregion

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!( isDead || isPaused )) {
                switch (state) {
                    case AIState.SLEEP:
                        SleepUpdate();
                        break;
                    case AIState.WATCH:
                        WatchUpdate();
                        break;
                    case AIState.REACH:
                        ReachUpdate();
                        break;
                    case AIState.FOLLOW:
                        FollowUpdate();
                        break;
                    case AIState.ATTACK:
                        AttackUpdate();
                        break;
                }
            }
        }

        /// <summary>
        /// Moves towards current direction
        /// </summary>
        /// <param name="moveSpeed">Moving speed, if less or equal 0 then there is no movement</param>
        public virtual void Move(float moveSpeed)
        {
            if (moveSpeed <= 0 && !canMove)
                return;
            transform.Translate( moveDirection * moveSpeed * Time.fixedDeltaTime, Space.World );
        }

        public virtual void SetPosToFollow(Vector2 position)
        {
            if (dummyPositionToMove != null) {
                dummyPositionToMove.gameObject.SetActive( false );
                dummyPositionToMove = null;
            }
            dummyPositionToMove = AIManager.GetDummy( position );
            currentTarget = dummyPositionToMove.transform;
            SetMoveDirection( position );
        }

        public virtual void SetMoveDirection(Entity target)
        {
            SetMoveDirection( target.transform.position );
        }

        /// <summary>
        /// Updates move direction depends on given position. It only takes x value from vector
        /// </summary>
        /// <param name="position">Position</param>
        public virtual void SetMoveDirection(Vector2 position)
        {
            SetMoveDirection( position.x );
        }

        /// <summary>
        /// Update move direction depdens on given parameter, x > 0 gives 1, other values gives -1
        /// </summary>
        /// <param name="x">X value used for move direction vector</param>
        public virtual void SetMoveDirection(float x)
        {
            float dir = x - transform.position.x;
            moveDirection = new Vector2( ( dir > 0 ? 1 : -1 ), 0 );
        }

        /// <summary>
        /// Gives next node to follow in AI path
        /// </summary>
        public virtual void NextNode()
        {
            if (!PathHasEnded) {
                currentCn = currentPath.Next();
                currentTarget = currentCn.Node.transform;
                switch (currentCn.Action) {
                    case AIAction.MOVE:
                        SetMoveDirection( currentCn.Node.transform.position );
                        break;
                    case AIAction.JUMP:
                        // JUMP ACTION
                        break;
                }
            } else {
                currentPath.Clear();
                currentCn = null;
                currentTarget = entityToFollowAfterPath.transform;
                SetFollowState( "path has ended and now is time to reach last target" );
            }
        }

        public virtual bool MoveToPosition(Vector2 position)
        {
            dummyPositionToMove = AIManager.GetDummy( position );
            return FollowTarget( dummyPositionToMove );
        }

        /// <summary>
        /// Start following target straight in line
        /// </summary>
        /// <param name="target">Target to follow</param>
        public virtual bool FollowTarget(Entity target)
        {
            if (!SetPathTo( target )) {
                if (dummyPositionToMove != null) {
                    dummyPositionToMove.gameObject.SetActive( false );
                    dummyPositionToMove = null;
                }
                return false;
            }

            entityToFollowAfterPath = target;

            // JEŻELI JEST TYLKO JEDEN NODE TO OZNACZA, ŻE CEL JEST NA
            // TEJ SAMEJ WYSOKOŚCI
            if (currentPath.LeftNodes == 1) {
                ChaseTarget( entityToFollowAfterPath );
            } else {
                NextNode();
                SetReachState( "path has been found" );
            }
            return true;
        }

        public virtual bool ChaseTarget(Entity target)
        {
            if (followedEntity != null) {
                followedEntity.RemoveFollower( this );
            }
            if (target.ContactArea != ContactArea) {
                Debug.LogError( "this shouldn't happen" );
                return false;
            }
            target.AddFollower( this );
            followedEntity = target;
            SetMoveDirection( followedEntity );
            SetFollowState( "set througth method ChaseTarget" );
            return true;
        }

        /// <summary>
        /// Checks if given node is in AI path
        /// </summary>
        /// <param name="node">Node to check</param>
        /// <returns><code>TRUE</code> if node is in path, otherwise <code>FALSE</code></returns>
        public bool IsNodeInPath(Node node)
        {
            return currentPath.Contains( node );
        }

        public virtual void SetSleepState(string reason = null)
        {
            state = AIState.SLEEP;
            OnAnyStateChange( reason );
        }

        public virtual void SetWatchState(string reason = null)
        {
            state = AIState.WATCH;
            OnAnyStateChange( reason );
        }

        public virtual void SetReachState(string reason = null)
        {
            state = AIState.REACH;
            OnAnyStateChange( reason );
            reachDirectionUpdate = 0;
        }

        public virtual void SetAttackState(string reason = null)
        {
            state = AIState.ATTACK;
            OnAnyStateChange( reason );
        }

        public virtual void SetFollowState(string reason = null)
        {
            state = AIState.FOLLOW;
            OnAnyStateChange( reason );
            followDirectionUpdate = 0;
        }

        protected virtual void OnAnyStateChange(string reason)
        {
            Debug.Log( name + ", changed state to: " + state + ", reason: " + ( ( reason == null || reason.Equals( "" ) ) ? UNDEFINED : reason ) );
        }

        public override void OnFallen(float speedWhenFalling)
        {
            base.OnFallen( speedWhenFalling );
            moveSpeed.UpdateAttribute();
        }

        protected virtual void Jump(float jumpPower, float direction)
        {
            rb.velocity = new Vector2( rb.velocity.x, jumpPower );
            lastMinFallSpeed = 0;
        }

        /// <summary>
        /// Updates every one fixed update step
        /// </summary>
        public virtual void SleepUpdate()
        {
            OnAnyStateUpdate();
        }

        /// <summary>
        /// Updates every one fixed update step. Watch update should be like routing entity standard
        /// path or just to select random position to move.
        /// </summary>
        public virtual void WatchUpdate()
        {
            OnAnyStateUpdate();
        }

        /// <summary>
        /// Updates every one fixed update step
        /// </summary>
        public virtual void ReachUpdate()
        {
            OnAnyStateUpdate();
            reachDirectionUpdate++;
            if (reachDirectionUpdate == REACH_DIRECTION_UPDATE_PERIOD) {
                reachDirectionUpdate = 0;
                SetMoveDirection( currentTarget.transform.position );
            }
            Move( moveSpeed.current );
        }

        /// <summary>
        /// Updates every one fixed update step
        /// </summary>
        public virtual void FollowUpdate()
        {
            OnAnyStateUpdate();
            followDirectionUpdate++;
            if (followDirectionUpdate == FOLLOW_DIRECTION_UPDATE_PERIOD) {
                SetMoveDirection( followedEntity );
                followDirectionUpdate = 0;
            }
            Move( moveSpeed.current );
        }

        /// <summary>
        /// Updates every one fixed update step
        /// </summary>
        public virtual void AttackUpdate()
        {
            OnAnyStateUpdate();
        }

        public virtual void OnFollowedEntityChangesContactArea(Entity followed)
        {
            followed.RemoveFollower( this );
        }

        /// <summary>
        /// Updates every one fixed update step
        /// </summary>
        public abstract void OnAnyStateUpdate();

        protected virtual bool SetPathTo(Entity entity)
        {
            // REMOVES DUMMY IF EXISTS
            if (dummyPositionToMove != null) {
                dummyPositionToMove.gameObject.SetActive( false );
                dummyPositionToMove = null;
            }
            // RESET ALL FIELDS THAT ARE USED FOR AI MOVEMENT
            currentPath.Clear();
            currentCn = null;
            entityToFollowAfterPath = null;
            AIPathList pathList = AIManager.FindPath( this, entity );
            if (pathList != null) {
                currentPath = pathList;
                return true;
            }
            return false;
        }

        #region Getters and Setters
        /// <summary>
        /// <code>TRUE</code> if AI has path, otherwise <code>FALSE</code>
        /// </summary>
        public bool HasPath { get => !currentPath.IsFullyEmpty; }
        /// <summary>
        /// <code>TRUE</code> if AI path has ended, otherwise <code>FALSE</code>
        /// </summary>
        public bool PathHasEnded { get => currentPath.IsEmpty; }
        public ComplexNode CurrentComplexNode { get => currentCn; }
        /// <summary>
        /// State of enemy's AI
        /// </summary>
        public AIState State { get => state; }
        #endregion
    }
}
