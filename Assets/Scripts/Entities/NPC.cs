using DoubleMMPrjc.AI;
using DoubleMMPrjc.Timer;
using UnityEngine;

namespace DoubleMMPrjc
{
    public abstract class NPC : Entity
    {
        protected AIPathList currentPath = new AIPathList();
        [SerializeField] protected Attribute jumpSpeed = new Attribute();

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

        protected bool landed = true;
        protected static readonly float JUMP_Y_OFFSET = 1.2f;
        protected static readonly float JUMP_TIME = 0.75f;

        protected static readonly int FOLLOW_DIRECTION_UPDATE_PERIOD = 8;
        protected int followDirectionUpdate = 0;

        protected static readonly int REACH_DIRECTION_UPDATE_PERIOD = 12;
        protected int reachDirectionUpdate = 0;

        protected static readonly int POSITION_CHECK_PERIOD = 5;
        protected int positionCheck = 0;
        protected bool positionReached = false;
        protected bool xPositionReached = false;

        protected long refindPathCountdownId;

        [SerializeField] private AIState state = AIState.SLEEP;
        [SerializeField] private AIMovingState movingState = AIMovingState.WALKING;
        [SerializeField] private bool logStatusChange = true;
        private int logStatusCount = 1;

        #region DEV SECTION
        private static readonly string UNDEFINED = "UNDEFINED";
        protected string stateReasonChange = UNDEFINED;
        #endregion

        #region Unity API
        public override void Start()
        {
            base.Start();
            refindPathCountdownId = TimerManager.Create( this );
        }

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
        #endregion

        #region StateSetter
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
            if (logStatusChange) {
                Debug.Log( "(" + logStatusCount + ") " + name + ", changed state to: "
                    + state + ", reason: " + ( ( reason == null || reason.Equals( "" ) ) ? UNDEFINED : reason ) );
                logStatusCount++;
            }
        }
        #endregion

        #region StateUpdates
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

            if (!xPositionReached) {
                if (Mathf.Abs( currentTarget.position.x - transform.position.x ) < 0.25f) {
                    xPositionReached = true;
                } else {
                    SetMoveDirection( currentTarget.transform.position );
                }
            }

            positionCheck++;
            if (positionCheck >= POSITION_CHECK_PERIOD) {
                positionCheck = 0;
                if (Vector2.Distance( currentTarget.position, transform.position ) <= 0.5f) {
                    NextNode();
                    return;
                }
            }

            if (!xPositionReached) {
                if (landed) {
                    Move( moveSpeed.current );
                } else {
                    Move( jumpSpeed.current );
                }
            }
        }

        public virtual void FollowUpdate()
        {
            OnAnyStateUpdate();
            followDirectionUpdate++;
            if (followDirectionUpdate == FOLLOW_DIRECTION_UPDATE_PERIOD) {
                /*  if ( followedEntity.ContactArea != ContactArea ) {
                      if ( !FollowTarget(followedEntity) && TimerManager.HasEnded(reachCountdownId) ) {
                          SetWatchState();
                      }
                      return;
                  }*/
                SetMoveDirection( followedEntity );
                followDirectionUpdate = 0;
            }
            Move( moveSpeed.current );
        }

        public virtual void AttackUpdate()
        {
            OnAnyStateUpdate();
        }

        public abstract void OnAnyStateUpdate();
        #endregion

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
                positionReached = false;
                xPositionReached = false;
                currentCn = currentPath.Next();
                currentTarget = currentCn.Node.transform;
                switch (currentCn.Action) {
                    case AIAction.WALK:
                        movingState = AIMovingState.WALKING;
                        SetMoveDirection( currentCn.Node.transform.position );
                        break;
                    case AIAction.JUMP:           
                        // Sets jump power that depdens on height that must be reached
                        float heigth = currentTarget.position.y - transform.position.y;
                        float jumpPower = heigth / JUMP_TIME - Physics.gravity.y / 2f * JUMP_TIME;
                        if ( jumpPower < 0 ) {
                            jumpPower = 0;
                        }
                        JumpTo( jumpPower, currentTarget.position );
                        break;
                }
            } else {
                currentPath.Clear();
                currentCn = null;
                if (entityToFollowAfterPath.ContactArea != ContactArea ) {
                    FollowTarget(entityToFollowAfterPath);
                } else {
                    currentTarget = entityToFollowAfterPath.transform;
                    SetFollowState( "path has ended and now is time to reach last target" );
                }
            }
        }

        /*  protected bool ReachedCurrentNode()
          {
              if (currentCn == null || currentCn.Node == null)
                  return false;

          }*/

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
            if (target == null || ContactArea == null) {
                return false;
            }

            if (ChaseTarget( target )) {
                return true;
            }

            if (!SetPathTo( target )) {
                if (dummyPositionToMove != null) {
                    dummyPositionToMove.gameObject.SetActive( false );
                    dummyPositionToMove = null;
                }
                return false;
            }

            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }
            entityToFollowAfterPath = target;
            followedEntity = target;
            target.AddFollower( this );

            NextNode();
            SetReachState( "found path to target" );
            TimerManager.Stop( refindPathCountdownId );

            return true;
        }

        public void OnDrawGizmos()
        {
            if ( !isDead ) {
                DrawGizmos();
            }
        }

        public abstract void DrawGizmos();

        /// <summary>
        /// Chase/follow to given target if only it's has same contact area
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool ChaseTarget(Entity target)
        {
            // Remove old followed entity from list
            if (followedEntity) {
                followedEntity.RemoveFollower( this );
            }

            // Cannot chase target when is in other area
            if (target.ContactArea != ContactArea) {
                return false;
            }

            // Can chase, set up some variables and change state to follow
            target.AddFollower( this );
            followedEntity = target;
            currentTarget = target.transform;
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

        public override void OnFallen(float speedWhenFalling)
        {
            base.OnFallen( speedWhenFalling );
            movingState = AIMovingState.WALKING;
            landed = true;
        }

        public virtual void JumpTo(float jumpPower, Vector2 end)
        {
            // Sets direction
            SetMoveDirection( end );
            // Sets flag that npc started jumping
            landed = false;
            // Calculating move speed depdends on time need to reach x position of node
            float moveSpeed = ( end.x - transform.position.x ) / JUMP_TIME;
            jumpSpeed.current = Mathf.Abs( moveSpeed );
            movingState = AIMovingState.JUMPING;
            Jump(jumpPower);
        }

        public virtual bool CanJumpFromTo(float jumpPower, Vector2 start, Vector2 end)
        {
            float heigth = end.y - start.y;
            return (heigth + JUMP_Y_OFFSET) <= (jumpPower * jumpPower / ( 2 * Mathf.Abs( Physics.gravity.y ) ));
        }

        public virtual void OnFollowedEntityChangesContactArea(Entity followed)
        {
            followed.RemoveFollower( this );
        }

        /*
        public virtual void OnNodeEnter(Node node)
        {
            if (CurrentComplexNode != null && CurrentComplexNode.Node == node) {
                NextNode();
            }
        }

        public virtual void OnNodeExit(Node node)
        {

        }*/

        public virtual void StartPathRefind(float interval)
        {
            TimerManager.Reset( refindPathCountdownId, interval );
        }

        public override void Die()
        {
            base.Die();
            currentTarget = null;
            if (dummyPositionToMove != null) {
                dummyPositionToMove.gameObject.SetActive( false );
                dummyPositionToMove = null;
            }
        }

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
            //23.06.19
            //entityToFollowAfterPath = null;
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
        public AIMovingState MovingState { get => movingState; }
        #endregion
    }
}
 