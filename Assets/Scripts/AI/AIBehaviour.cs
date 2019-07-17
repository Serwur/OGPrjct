using DoubleMMPrjc.Timer;
using UnityEngine;
namespace DoubleMMPrjc
{
    namespace AI
    {
        public interface IStateAIListener
        {
            void SetSleepState(AIState oldState);
            void SetWatchState(AIState oldState);
            void SetReachState(AIState oldState);
            void SetAttackState(AIState oldState);
            void SetFollowState(AIState oldState);
            void OnAnyStateChange(AIState oldState, AIState newState);

            void SleepUpdate();
            void WatchUpdate();
            void ReachUpdate();
            void FollowUpdate();
            void AttackUpdate();
            void OnAnyStateUpdate(AIState currentState);
        }

        [RequireComponent( typeof( NPC ) )]
        public class AIBehaviour : MonoBehaviour, IOnCountdownEnd
        {
            protected NPC owner;
            protected IStateAIListener stateListener;
            protected AIPathList currentPath = new AIPathList();
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
            protected bool paused = false;

            protected long refindPathCountdownId;

            [SerializeField] private AIState state = AIState.SLEEP;
            [SerializeField] private AIMovingState movingState = AIMovingState.WALKING;

            #region DEV SECTION
            [SerializeField] private bool logs = true;
            private int logStatusCount = 1;
            private static readonly string UNDEFINED = "UNDEFINED";
            private string stateReasonChange = UNDEFINED;
            #endregion

            #region Unity API
            public void Awake()
            {
                owner = GetComponent<NPC>();
                stateListener = GetComponent<IStateAIListener>();
                if (stateListener == null) {
                    Debug.LogWarning( name + ": State handling is without listener" );
                }
            }

            public void Start()
            {
                refindPathCountdownId = TimerManager.Create( this );
            }

            public void FixedUpdate()
            {
                if (!paused)
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
            #endregion


            #region StateSetter
            public virtual void SetSleepState(AIState oldState, string reason = null)
            {
                state = AIState.SLEEP;
                if (stateListener != null)
                    stateListener.SetSleepState( oldState );
                OnAnyStateChange( oldState, state, reason );
            }

            public virtual void SetWatchState(AIState oldState, string reason = null)
            {
                state = AIState.WATCH;
                if (stateListener != null)
                    stateListener.SetSleepState( oldState );
                OnAnyStateChange( oldState, state, reason );
            }

            public virtual void SetReachState(AIState oldState, string reason = null)
            {
                state = AIState.REACH;
                if (stateListener != null)
                    stateListener.SetSleepState( oldState );
                OnAnyStateChange( oldState, state, reason );
                reachDirectionUpdate = 0;
            }

            public virtual void SetAttackState(AIState oldState, string reason = null)
            {
                state = AIState.ATTACK;
                if (stateListener != null)
                    stateListener.SetSleepState( oldState );
                OnAnyStateChange( oldState, state, reason );
            }

            public virtual void SetFollowState(AIState oldState, string reason = null)
            {
                state = AIState.FOLLOW;
                if (stateListener != null)
                    stateListener.SetSleepState( oldState );
                OnAnyStateChange( oldState, state, reason );
                followDirectionUpdate = 0;
            }

            protected virtual void OnAnyStateChange(AIState oldState, AIState newState, string reason)
            {
                if (stateListener != null)
                    stateListener.OnAnyStateChange( oldState, newState );
                if (logs) {
                    Debug.Log( "(" + logStatusCount + ") " + name + ", changed state to: "
                        + newState + " from " + oldState + ", reason: "
                        + ( ( reason == null || reason.Equals( "" ) ) ? UNDEFINED : reason ) );
                    logStatusCount++;
                }
            }
            #endregion

            #region StateUpdates
            public virtual void SleepUpdate()
            {
                if (stateListener != null)
                    stateListener.OnAnyStateUpdate( State );
            }

            public virtual void WatchUpdate()
            {
                if (stateListener != null)
                    stateListener.OnAnyStateUpdate( State );
            }

            public virtual void ReachUpdate()
            {
                if (stateListener != null)
                    stateListener.OnAnyStateUpdate( State );

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
                        owner.Move( owner.moveSpeed.current );
                    } else {
                        owner.Move( owner.jumpSpeed.current );
                    }
                }
            }

            public virtual void FollowUpdate()
            {
                if (stateListener != null)
                    stateListener.OnAnyStateUpdate( State );

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
                owner.Move( owner.moveSpeed.current );
            }

            public virtual void AttackUpdate()
            {
                if (stateListener != null)
                    stateListener.OnAnyStateUpdate( State );
            }

            #endregion

            protected void SetMoveDirection(Entity target)
            {
                SetMoveDirection( target.transform.position );
            }

            /// <summary>
            /// Updates move direction depends on given position. It only takes x value from vector
            /// </summary>
            /// <param name="position">Position</param>
            protected void SetMoveDirection(Vector2 position)
            {
                SetMoveDirection( position.x );
            }

            /// <summary>
            /// Update move direction depdens on given parameter, x > 0 gives 1, other values gives -1
            /// </summary>
            /// <param name="x">X value used for move direction vector</param>
            protected void SetMoveDirection(float x)
            {
                float dir = x - transform.position.x;
                owner.TurnTo( x );
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
                            if (jumpPower < 0) {
                                jumpPower = 0;
                            }
                            JumpTo( jumpPower, currentTarget.position );
                            break;
                    }
                } else {
                    currentPath.Clear();
                    currentCn = null;
                    if (entityToFollowAfterPath.ContactArea != owner.ContactArea) {
                        FollowTarget( entityToFollowAfterPath );
                    } else {
                        currentTarget = entityToFollowAfterPath.transform;
                        SetFollowState( state, "path has ended and now is time to reach last target" );
                    }
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
                if (target == null || owner.ContactArea == null) {
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
                    followedEntity.RemoveFollower( owner );
                }
                entityToFollowAfterPath = target;
                followedEntity = target;
                target.AddFollower( owner );

                NextNode();
                SetReachState( state, "found path to target" );
                TimerManager.Stop( refindPathCountdownId );

                return true;
            }

            /// <summary>
            /// Chase/follow to given target if only it's has same contact area
            /// </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            public virtual bool ChaseTarget(Entity target)
            {
                // Remove old followed entity from list
                if (followedEntity) {
                    followedEntity.RemoveFollower( owner );
                }

                // Cannot chase target when is in other area
                if (target.ContactArea != owner.ContactArea) {
                    return false;
                }

                // Can chase, set up some variables and change state to follow
                target.AddFollower( owner );
                followedEntity = target;
                currentTarget = target.transform;
                SetMoveDirection( followedEntity );
                SetFollowState( state, "set througth method ChaseTarget" );

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

            public virtual void JumpTo(float jumpPower, Vector2 end)
            {
                // Sets direction
                SetMoveDirection( end );
                // Sets flag that npc started jumping
                landed = false;
                // Calculating move speed depdends on time need to reach x position of node
                float moveSpeed = ( end.x - transform.position.x ) / JUMP_TIME;
                owner.jumpSpeed.current = Mathf.Abs( moveSpeed );
                movingState = AIMovingState.JUMPING;
                owner.Jump( jumpPower );
            }

            protected bool CanJumpFromTo(float jumpPower, Vector2 start, Vector2 end)
            {
                float heigth = end.y - start.y;
                return ( heigth + JUMP_Y_OFFSET ) <= ( jumpPower * jumpPower / ( 2 * Mathf.Abs( Physics.gravity.y ) ) );
            }

            public virtual void StartPathRefind(float interval)
            {
                TimerManager.Reset( refindPathCountdownId, interval );
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
                AIPathList pathList = AIManager.FindPath( owner, entity );
                if (pathList != null) {
                    currentPath = pathList;
                    return true;
                }
                return false;
            }

            public void OnCountdownEnd(long id)
            {
                if (id == refindPathCountdownId && !FollowTarget( followedEntity )) {
                    TimerManager.Reset( refindPathCountdownId );
                }
            }

            public void Stop()
            {
                paused = true;
                currentTarget = null;
                if (dummyPositionToMove != null) {
                    AIManager.ReturnDummy( dummyPositionToMove );
                }
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
}