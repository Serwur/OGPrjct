using DoubleMMPrjc.AI;
using UnityEngine;

namespace DoubleMMPrjc
{
    public abstract class NPC : Entity
    {
        public static readonly int NO_TARGET = -1;
        public static readonly int STATIC_TARGET = 1;
        public static readonly int MOVING_TARGET = 0;

        protected Vector2 moveDirection;
        protected Entity targetToFollowAfterPath;
        protected Dummy dummyPositionToFollow;
        protected Transform currentTarget;
        /// <summary>
        /// Tells AI where should go if set to go somewhere:
        /// -1 = nowhere,
        /// 0 = to destinationReach <see cref="positionToFollowAfterPath"/>,
        /// 1 = to targetReach <see cref="currentTarget"/>
        /// </summary>
        protected int targetParameter = -1;
        protected AIPathList currentPath = new AIPathList();
        protected ComplexNode currentCn;

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
                    case AIState.ATTACK:
                        AttackUpdate();
                        break;
                }
            }
        }

        /// <summary>
        /// Moves towards current target with given speed
        /// </summary>
        /// <param name="moveSpeed"></param>
        public virtual void Move(float moveSpeed)
        {
            if (moveSpeed < 0)
                return;
            transform.Translate( moveDirection * moveSpeed * Time.fixedDeltaTime, Space.World );
        }

        public virtual bool FindPath(Entity target)
        {
            AIPathList newPath = AIManager.FindPath( this, target );
            if (newPath == null) {
                // PATH HASN'T BEEN FOUND
                currentPath.Clear();
                currentCn = null;
                targetToFollowAfterPath = null;
                targetParameter = NO_TARGET;
                return false;
            }
            // PATH HAS BEEN FOUND
            targetParameter = MOVING_TARGET;
            targetToFollowAfterPath = target;

            // JEŻELI JEST TYLKO JEDEN NODE TO OZNACZA, ŻE CEL JEST NA
            // TEJ SAMEJ WYSOKOŚCI
            if (newPath.LeftNodes == 1) {
                UpdateDirAfterPath();
            } else {
                currentPath = newPath;
                NextNode();
            }
            return true;
        }

        /// <summary>
        /// Updates move direction depends on current reach position or target to follow
        /// </summary>
        public virtual void UpdateDirAfterPath()
        {
            if (targetParameter == MOVING_TARGET) {
                currentTarget = targetToFollowAfterPath.transform;
                SetMoveDirection( currentTarget.transform.position );
            } else {
                Debug.LogError( "There is no target to follow. Set following target first via FollowTarget(Entity.class) method" );
            }
        }

        public virtual void SetPosToFollow(Vector2 position)
        {
            if (dummyPositionToFollow != null) {
                Destroy( dummyPositionToFollow.gameObject );
            }
            dummyPositionToFollow = AIManager.SpawnDummy( position );
            currentTarget = dummyPositionToFollow.transform;
            SetMoveDirection( position );
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
                UpdateDirAfterPath();
            }
        }

        /// <summary>
        /// Start following target straight in line
        /// </summary>
        /// <param name="entity">Target to follow</param>
        public virtual void FollowTarget(Entity entity)
        {
            currentPath.Clear();
            currentTarget = entity.transform;
            SetMoveDirection( currentTarget.position );
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
        }

        public virtual void SetAttackState(string reason = null)
        {
            state = AIState.ATTACK;
            OnAnyStateChange( reason );
        }

        public override void OnFallen(float speedWhenFalling)
        {
            base.OnFallen( speedWhenFalling );
            moveSpeed.UpdateAttribute();
        }

        protected virtual void OnAnyStateChange(string reason)
        {
            Debug.Log( name + ", changed state to: " + state + ", reason: " + ( ( reason == null || reason.Equals( "" ) ) ? UNDEFINED : reason ) );
        }

        protected virtual void Jump(Node node)
        {
            float jumpDirection = node.transform.position.x - transform.position.x;

        }

        protected virtual void Jump(float jumpPower, float direction)
        {
            rb.velocity = new Vector2( rb.velocity.x, jumpPower );
            lastMinFallSpeed = 0;
        }

        public virtual void SleepUpdate()
        {
            OnAnyStateUpdate();
        }

        public virtual void WatchUpdate()
        {
            OnAnyStateUpdate();
        }

        public virtual void ReachUpdate()
        {
            OnAnyStateUpdate();
        }

        public virtual void AttackUpdate()
        {
            OnAnyStateUpdate();
        }

        public abstract void OnAnyStateUpdate();

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
