using ColdCry.Objects;
using ColdCry.Utility;
using UnityEngine;

namespace ColdCry.AI
{
    [RequireComponent( typeof( Entity ) )]
    public class AIBehaviour : MonoBehaviour, IOnCountdownEnd
    {

        [SerializeField] private float refindInterval = 0.75f;

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

        #region DEV SECTION
        [SerializeField] private bool logs = true;
        private int logStatusCount = 1;
        private static readonly string UNDEFINED = "UNDEFINED";
        private string stateReasonChange = UNDEFINED;
        #endregion

        #region Unity API
        public void Awake()
        {
            Owner = GetComponent<NPC>();

            StateListener = GetComponent<IStateAIListener>();
            if (StateListener == null) {
                Debug.LogWarning( name + " is without state listener" );
            }

            LateStateListener = GetComponent<ILateStateAIListener>();
            if (LateStateListener == null) {
                Debug.LogWarning( name + " is without late state listener" );
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
                    case AIState.REACH:
                        ReachUpdate();
                        break;
                    case AIState.FOLLOW:
                        FollowUpdate();
                        break;
                    case AIState.WAIT:
                        WaitUpdate();
                        break;
                    case AIState.SLEEP:
                        SleepUpdate();
                        break;
                }

        }
        #endregion

        #region StateSetter
        public virtual void SetReachState(AIState oldState, string reason = null)
        {
            state = AIState.REACH;
            if (StateListener != null)
                StateListener.SetReachState( oldState );

            OnAnyStateChange( oldState, state, reason );
            reachDirectionUpdate = 0;

            if (LateStateListener != null)
                LateStateListener.SetLateReachState( oldState );
        }

        public virtual void SetFollowState(AIState oldState, string reason = null)
        {
            state = AIState.FOLLOW;
            if (StateListener != null)
                StateListener.SetFollowState( oldState );

            OnAnyStateChange( oldState, state, reason );
            followDirectionUpdate = 0;

            if (LateStateListener != null)
                LateStateListener.SetLateFollowState( oldState );
        }

        public virtual void SetWaitState(AIState oldState, string reason = null)
        {
            state = AIState.FOLLOW;
            if (StateListener != null)
                StateListener.SetFollowState( oldState );

            OnAnyStateChange( oldState, state, reason );
            followDirectionUpdate = 0;

            if (LateStateListener != null)
                LateStateListener.SetLateFollowState( oldState );
        }

        public virtual void SetSleepState(AIState oldState, string reason = null)
        {
            state = AIState.FOLLOW;
            if (StateListener != null)
                StateListener.SetFollowState( oldState );

            OnAnyStateChange( oldState, state, reason );
            followDirectionUpdate = 0;

            if (LateStateListener != null)
                LateStateListener.SetLateFollowState( oldState );
        }


        public virtual void OnAnyStateChange(AIState oldState, AIState newState, string reason)
        {
            if (StateListener != null)
                StateListener.OnAnyStateChange( oldState, newState );

            if (LateStateListener != null)
                LateStateListener.SetLateFollowState( oldState );

            if (logs) {
                Debug.Log( "(" + logStatusCount + ") " + name + ", changed state to: "
                    + newState + " from " + oldState + ", reason: "
                    + ( ( reason == null || reason.Equals( "" ) ) ? UNDEFINED : reason ) );
                logStatusCount++;
            }
        }
        #endregion

        #region StateUpdates
        protected virtual void ReachUpdate()
        {
            if (StateListener != null)
                StateListener.OnAnyStateUpdate( State );

            positionCheck++;
            if (positionCheck >= POSITION_CHECK_PERIOD) {
                positionCheck = 0;
                if (Vector2.Distance( CurrentTarget.position, transform.position ) <= 0.5f) {
                    NextNode();
                    return;
                }
            }

            if (!xPositionReached) {
                Owner.Move( CurrentTarget );
            }
        }

        protected virtual void FollowUpdate()
        {
            if (StateListener != null)
                StateListener.OnAnyStateUpdate( State );

            Owner.Move( Owner.MoveSpeed.Current, FollowedEntity );
        }

        protected virtual void SleepUpdate()
        {
        }

        protected virtual void WaitUpdate()
        {
        }
        #endregion

        /// <summary>
        /// If <b>true</b> then pauses all action that is being do by this component.
        /// Otherwise it backs to previous actions.
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            paused = pause;
        }

        /// <summary>
        /// Stops all actions, deletes references and back to SLEEP state
        /// </summary>
        public void Stop()
        {
            paused = true;
            CurrentTarget = null;
            if (DummyPositionToMove != null) {
                AIManager.ReturnDummy( DummyPositionToMove );
                DummyPositionToMove = null;
            }
        }

        public virtual bool MoveToPosition(Vector2 position)
        {
            DummyPositionToMove = AIManager.GetDummy( position );
            return FollowTarget( DummyPositionToMove );
        }

        /// <summary>
        /// Start following target straight in line
        /// </summary>
        /// <param name="target">Target to follow</param>
        public virtual bool FollowTarget(Entity target)
        {
            if (target == null || Owner.ContactArea == null) {
                return false;
            }

            if (ChaseTarget( target )) {
                return true;
            }

            if (!SetPathTo( target )) {
                if (DummyPositionToMove != null) {
                    DummyPositionToMove.gameObject.SetActive( false );
                    DummyPositionToMove = null;
                }
                return false;
            }

            if (FollowedEntity) {
                FollowedEntity.RemoveFollower( this );
            }
            EntityToFollowAfterPath = target;
            FollowedEntity = target;
            target.AddFollower( this );

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
            if (FollowedEntity) {
                FollowedEntity.RemoveFollower( this );
            }

            // Cannot chase target when is in other area
            if (target.ContactArea != Owner.ContactArea) {
                return false;
            }

            // Can chase, set up some variables and change state to follow
            target.AddFollower( this );
            FollowedEntity = target;
            CurrentTarget = target.transform;
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
            return CurrentPath.Contains( node );
        }

        /// <summary>
        /// Start an interval timer which calculate the shortest path to Current target.
        /// It stops when the path is found or if it exceeds <b>default</b> break time 
        /// which is 7.5s.
        /// If Current target is null then it won't start.
        /// </summary>
        /// <param name="interval">Time in seconds to repeat</param>
        /// <returns></returns>
        public virtual bool StartPathRefind(float interval)
        {
            //if ( curr)
            TimerManager.Reset( refindPathCountdownId, interval );
            return true;
        }

        /// <summary>
        /// Start an interval timer which calculate the shortest path to Current target.
        /// It stops when the path is found or if it exceeds given break time.
        /// If Current target is null then it won't start.
        /// </summary>
        /// <param name="interval">Time in seconds to repeat</param>
        /// <param name="breakTime">Time after scheduled refinding will broke</param>
        /// <returns></returns>
        public virtual bool StartPathRefind(float interval, float breakTime)
        {
            TimerManager.Reset( refindPathCountdownId, interval );
            return true;
        }

        public virtual void OnCountdownEnd(long id, float overtime)
        {
            if (id == refindPathCountdownId && !FollowTarget( FollowedEntity )) {
                TimerManager.Reset( refindPathCountdownId );
            }
        }

        /// <summary>
        /// Gives next node to follow in AI path
        /// </summary>
        protected virtual void NextNode()
        {
            if (!PathHasEnded) {
                positionReached = false;
                xPositionReached = false;
                CurrentCn = CurrentPath.Next();
                CurrentTarget = CurrentCn.Node.transform;
                switch (CurrentCn.Action) {
                    case AIAction.WALK:
                        break;
                    case AIAction.JUMP:
                        // Sets jump power that depdens on height that must be reached
                        float heigth = CurrentTarget.position.y - transform.position.y;
                        float jumpPower = heigth / JUMP_TIME - Physics.gravity.y / 2f * JUMP_TIME;
                        if (jumpPower < 0) {
                            jumpPower = 0;
                        }
                        JumpTo( jumpPower, CurrentTarget.position );
                        break;
                }
            } else {
                CurrentPath.Clear();
                CurrentCn = null;
                if (EntityToFollowAfterPath.ContactArea != Owner.ContactArea) {
                    FollowTarget( EntityToFollowAfterPath );
                } else {
                    CurrentTarget = EntityToFollowAfterPath.transform;
                    SetFollowState( state, "path has ended and now is time to reach last target" );
                }
            }
        }

        /// <summary>
        /// Jumps to given position even it won't reach it. It uses parabolla trajectory.
        /// </summary>
        /// <param name="jumpPower">Power of jump</param>
        /// <param name="end">Position to jump to</param>
        protected virtual void JumpTo(float jumpPower, Vector2 end)
        {
            // Calculating move speed depdends on time need to reach x position of node
            float moveSpeed = ( end.x - transform.position.x ) / JUMP_TIME;
            Owner.FlySpeed.Current = Mathf.Abs( moveSpeed );
            Owner.Jump( jumpPower );
        }

        /// <summary>
        /// Checks if entity can jump to given position from given start position.
        /// </summary>
        /// <param name="jumpPower">power of entity jump</param>
        /// <param name="start">start position</param>
        /// <param name="end">end position</param>
        /// <returns><b>True</b> if entity can jump to given position otherwise returns <b>false</b></returns>
        protected bool CanJumpFromTo(float jumpPower, Vector2 start, Vector2 end)
        {
            float heigth = end.y - start.y;
            return ( heigth + JUMP_Y_OFFSET ) <= ( jumpPower * jumpPower / ( 2 * Mathf.Abs( Physics.gravity.y ) ) );
        }

        /// <summary>
        /// Sets path to given entity and returns if it can reach given target
        /// </summary>
        /// <param name="entity">Entity to reach</param>
        /// <returns><b>True</b> of entity can reach given target, otherwise returns <b>false</b></returns>
        protected virtual bool SetPathTo(Entity entity)
        {
            // REMOVES DUMMY IF EXISTS
            if (DummyPositionToMove != null) {
                DummyPositionToMove.gameObject.SetActive( false );
                DummyPositionToMove = null;
            }
            // RESET ALL FIELDS THAT ARE USED FOR AI MOVEMENT
            CurrentPath.Clear();
            CurrentCn = null;
            //23.06.19
            //entityToFollowAfterPath = null;
            AIPathList pathList = AIManager.FindPath( Owner, entity );
            if (pathList != null) {
                CurrentPath = pathList;
                return true;
            }
            return false;
        }

        public void OnDisable()
        {
            Stop();
            Owner.ResetUnit();
        }

        #region Getters and Setters
        /// <summary>
        /// <code>TRUE</code> if AI has path, otherwise <code>FALSE</code>
        /// </summary>
        public bool HasPath { get => !CurrentPath.IsFullyEmpty; }
        /// <summary>
        /// <code>TRUE</code> if AI path has ended, otherwise <code>FALSE</code>
        /// </summary>
        public bool PathHasEnded { get => CurrentPath.IsEmpty; }
        public ComplexNode CurrentComplexNode { get => CurrentCn; }
        /// <summary>
        /// State of AI
        /// </summary>
        public AIState State { get => state; }
        /// <summary>
        /// Moving state of AI
        /// </summary>
        public float RefindInterval { get => refindInterval; set => refindInterval = value; }
        public Entity Owner { get; set; }
        public IStateAIListener StateListener { get; set; }
        public ILateStateAIListener LateStateListener { get; set; }
        public AIPathList CurrentPath { get; set; } = new AIPathList();
        public Entity EntityToFollowAfterPath { get; set; }
        public Entity FollowedEntity { get; set; }
        public Transform CurrentTarget { get; set; }
        public Dummy DummyPositionToMove { get; set; }
        public ComplexNode CurrentCn { get; set; }
        #endregion
    }
}