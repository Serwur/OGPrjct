using ColdCry.Objects;
using ColdCry.Utility;
using ColdCry.Utility.Patterns.Memory;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.AI
{
    [RequireComponent( typeof( Entity ) )]
    public class AIMovementBehaviour :
        MonoBehaviour,
        IOnCountdownEnd,
        IObservable<AIMovementState>,
        IMemoriable
    {
        private static readonly float JUMP_Y_OFFSET = 1.2f;
        private static readonly float JUMP_TIME = 0.75f;
        private static readonly int POSITION_CHECK_PERIOD = 5;

        [SerializeField] private float refindInterval = 0.75f;

        #region Unity API
        public void Awake()
        {
            Owner = GetComponent<Entity>();

            /*
            StateListener = GetComponent<IStateAIListener>();
            if (StateListener == null) {
                Debug.LogWarning( name + " is without state listener" );
            }

            LateStateListener = GetComponent<ILateStateAIListener>();
            if (LateStateListener == null) {
                Debug.LogWarning( name + " is without late state listener" );
            }*/

            Memory = new Memorier( this );
        }

        public void Start()
        {
            RefindPathCountdownId = TimerManager.CreateSchedule( refindInterval , this, 10 );
        }

        public void FixedUpdate()
        {
            if (Owner.CanMove && !Paused)
                switch (MovementState) {
                    case AIMovementState.NONE:
                        // TODO
                        break;
                    case AIMovementState.WAITING:
                        // TODO
                        break;
                    case AIMovementState.PATHING:
                        PathUpdate();
                        break;
                    case AIMovementState.FOLLOWING:
                        FollowUpdate();
                        break;
                }
        }
        #endregion

        #region StateUpdates
        private void PathUpdate()
        {
            PositionCheck++;
            if (PositionCheck >= POSITION_CHECK_PERIOD) {
                PositionCheck = 0;
                if (Vector2.Distance( CurrentTarget.position, transform.position ) <= 0.5f) {
                    NextNode();
                    return;
                }
            }

            // when entity in in air and x position has been reached we can't move him,
            // otherwise we move him anyway
            if (Owner.IsInAir) {
                if (!XPositionReached)
                    Owner.Move( CurrentTarget );
            } else {
                Owner.Move( CurrentTarget );
            }
        }

        private void FollowUpdate()
        {
            Owner.Move( FollowedEntity );
        }
        #endregion

        /// <summary>
        /// If <b>true</b> then pauses all action that is being do by this component.
        /// Otherwise it backs to previous actions.
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            Paused = pause;
        }

        /// <summary>
        /// Stops all actions, deletes references and back to SLEEP state
        /// </summary>
        public void Stop()
        {
            ClearCurrentTarget();
            ClearDummy();
            ClearPath();
            MovementState = AIMovementState.NONE;
            TargetType = AITargetType.NONE;
            Paused = false;
        }

        private void ClearDummy()
        {
            if (TemplateDummy != null) {
                AIManager.ReturnDummy( TemplateDummy );
                TemplateDummy = null;
            }
        }

        private void ClearCurrentTarget()
        {
            if (FollowedEntity != null) {
                FollowedEntity.RemoveFollower( this );
            }

            EntityToFollowAfterPath = null;

            if (CurrentTarget != null) {
                Entity entity = CurrentTarget.GetComponent<Entity>();
                if (entity != null) {
                    entity.RemoveFollower( this );
                }
            }

            CurrentTarget = null;
        }

        private void ClearPath()
        {
            CurrentPath.Clear();
            CurrentNode = null;
        }

        private AIMovementResponse FindPath(Entity target, bool keepOldTargetIfNotFound)
        {
            Memory.Save();

            if (TargetType != AITargetType.STEADY) {
                ClearDummy();
            }

            ClearCurrentTarget();
            ClearPath();

            // FAIL CASE
            if (target == null) {
                ClearDummy();
                TargetType = AITargetType.NONE;
                return AIMovementResponse.TARGET_NULL;
            }

            // FAIL CASE
            if (Owner.ContactArea == null) {
                ClearDummy();
                TargetType = AITargetType.NONE;
                return AIMovementResponse.NO_CONTACT_AREA;
            }

            // SUCCESS CASE
            if (target.ContactArea == Owner.ContactArea) {
                MovementState = AIMovementState.FOLLOWING;
                target.AddFollower( this );
                FollowedEntity = target;
                CurrentTarget = target.transform;
                TimerManager.Stop( RefindPathCountdownId );
                return AIMovementResponse.TARGET_IN_SAME_AREA;
            }

            CurrentPath = AIManager.FindPath( Owner, target );
            // FAIL CASE
            if (CurrentPath == null) {
                ClearDummy();
                TargetType = AITargetType.NONE;
                return AIMovementResponse.NO_PATH_TO_TARGET;
            }

            // SUCCESS CASE
            EntityToFollowAfterPath = target;
            FollowedEntity = target;
            target.AddFollower( this );
            MovementState = AIMovementState.PATHING;
            NextNode();
            TimerManager.Stop( RefindPathCountdownId );

            return AIMovementResponse.PATH_FOUND;
        }

        public AIMovementResponse ReachPosition(Vector2 position, bool keepOldPath = false)
        {
            if (TemplateDummy == null) {
                TemplateDummy = AIManager.GetDummy( position );
            } else {
                TemplateDummy.transform.position = position;
            }
            TargetType = AITargetType.STEADY;
            return FindPath( TemplateDummy, keepOldPath );
        }

        public AIMovementResponse ReachPosition(Transform transform, bool keepOldPath = false)
        {
            return ReachPosition( transform.position, keepOldPath );
        }

        public AIMovementResponse ReachPosition(Entity entity, bool keepOldPath = false)
        {
            return ReachPosition( entity.transform.position, keepOldPath );
        }

        public AIMovementResponse TrackTarget(Transform transform, bool keepOldPath = false)
        {
            Entity entity = transform.GetComponent<Entity>();
            if (entity == null)
                return AIMovementResponse.MISSING_ENTITY_COMPONENT;
            TargetType = AITargetType.MOVEABLE;
            return TrackTarget( entity, keepOldPath );
        }

        public AIMovementResponse TrackTarget(Entity entity, bool keepOldPath = false)
        {
            if (TemplateDummy != null) {
                AIManager.ReturnDummy( TemplateDummy );
                TemplateDummy = null;
            }
            return FindPath( entity, keepOldPath);
        }

        /*
        /// <summary>
        /// Chase/follow to given target if only it's has same contact area
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool TrackTarget(Entity target)
        {
        }
        */

        /// <summary>
        /// Checks if given node is in AI path
        /// </summary>
        /// <param name="node">Node to check</param>
        /// <returns><code>TRUE</code> if node is in path, otherwise <code>FALSE</code></returns>
        public bool IsNodeInPath(Node node)
        {
            return CurrentPath.Contains( node );
        }

        /*
        /// <summary>
        /// Start an interval timer which calculate the shortest path to Current target.
        /// It stops when the path is found or if it exceeds <b>default</b> break time 
        /// which is 7.5s.
        /// If Current target is null then it won't start.
        /// </summary>
        /// <param name="interval">Time in seconds to repeat</param>
        /// <returns></returns>
        public bool StartPathRefind(float interval)
        {
            //if ( curr)
            TimerManager.Reset( refindPathCountdownId, interval );
            return true;
        }*/

        /// <summary>
        /// Start an interval timer which calculate the shortest path to Current target.
        /// It stops when the path is found or if it exceeds given break time.
        /// If Current target is null then it won't start.
        /// </summary>
        /// <param name="interval">Time in seconds to repeat</param>
        /// <param name="breakTime">Time after scheduled refinding will broke</param>
        /// <returns></returns>
        public virtual bool StartPathRefind()
        {
            TimerManager.Reset( RefindPathCountdownId, RefindInterval );
            return true;
        }

        public virtual void OnCountdownEnd(long id, float overtime)
        {
            if (id == RefindPathCountdownId) {
                AIMovementResponse reponse = FindPath( FollowedEntity, true );
                TimerManager.Reset( RefindPathCountdownId );
            }
        }

        /// <summary>
        /// Gives next node to follow in AI path
        /// </summary>
        private void NextNode()
        {
            if (!CurrentPath.IsEmpty) {
                PositionReached = false;
                XPositionReached = false;
                CurrentNode = CurrentPath.Next();
                CurrentTarget = CurrentNode.Node.transform;
                switch (CurrentNode.Action) {
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
                ClearPath();
                if (EntityToFollowAfterPath.ContactArea != Owner.ContactArea) {
                    AIMovementResponse response = FindPath( EntityToFollowAfterPath, true );
                    switch (response) {
                        case AIMovementResponse.TARGET_NULL:
                            break;
                        case AIMovementResponse.NO_CONTACT_AREA:
                            break;
                        case AIMovementResponse.TARGET_IN_SAME_AREA:
                            break;
                        case AIMovementResponse.NO_PATH_TO_TARGET:
                            break;
                        case AIMovementResponse.PATH_FOUND:
                            break;
                        case AIMovementResponse.MISSING_ENTITY_COMPONENT:
                            break;
                    }
                } else {
                    CurrentTarget = EntityToFollowAfterPath.transform;
                }
            }
        }

        /// <summary>
        /// Jumps to given position even it won't reach it. It uses parabolla trajectory.
        /// </summary>
        /// <param name="jumpPower">Power of jump</param>
        /// <param name="end">Position to jump to</param>
        private void JumpTo(float jumpPower, Vector2 end)
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
        private bool CanJumpFromTo(float jumpPower, Vector2 start, Vector2 end)
        {
            float heigth = end.y - start.y;
            return ( heigth + JUMP_Y_OFFSET ) <= ( jumpPower * jumpPower / ( 2 * Mathf.Abs( Physics.gravity.y ) ) );
        }

        public void OnDisable()
        {
            Stop();
            Owner.ResetUnit();
        }

        public void OnDestroy()
        {
            Stop();
            Owner.ResetUnit();
        }

        public void Subscribe(IObserver<AIMovementState> observer)
        {
            if (!StateChangeObservers.Contains( observer ))
                StateChangeObservers.Add( observer );
        }

        public void Unsubscribe(IObserver<AIMovementState> observer)
        {
            StateChangeObservers.Remove( observer );
        }

        public MemoryPart SaveMemory()
        {
            MemoryPart part = new MemoryPart {
                { "MovementType", TargetType },
                { "MovementState", MovementState },
                { "MovementType", TargetType },
                { "MovementType", TargetType },
                { "MovementType", TargetType },
                { "MovementType", TargetType }
            };
            return part;
        }

        public void LoadMemory(MemoryPart data)
        {

        }

        #region Getters and Setters
        public AIPathList CurrentPath { get; private set; } = new AIPathList();

        public Entity Owner { get; private set; }
        public Entity EntityToFollowAfterPath { get; private set; }
        public Entity FollowedEntity { get; private set; }
        public Transform CurrentTarget { get; private set; }
        public Dummy TemplateDummy { get; private set; }
        public ComplexNode CurrentNode { get; private set; }
        public Memorier Memory { get; set; }
        public List<IObserver<AIMovementState>> StateChangeObservers { get; set; } = new List<IObserver<AIMovementState>>();

        public AITargetType TargetType { get; private set; }
        public AIMovementState MovementState { get; private set; }

        public float RefindInterval { get => refindInterval; set => refindInterval = value; }
        public bool HasPath { get => !CurrentPath.IsFullyEmpty; }
        public bool PathHasEnded { get => CurrentPath.IsEmpty; }
        public long RefindPathCountdownId { get; set; }
        public bool Paused { get; set; } = false;
        public bool XPositionReached { get; set; } = false;
        public bool PositionReached { get; set; } = false;
        public int PositionCheck { get; set; } = 0;
        #endregion
    }
}