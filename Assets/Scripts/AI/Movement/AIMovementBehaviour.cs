using ColdCry.AI.Movement;
using ColdCry.Core;
using ColdCry.Objects;
using ColdCry.Utility;
using ColdCry.Utility.Patterns.Memory;
using ColdCry.Utility.Time;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ColdCry.Utility.Time.TimerManager;

namespace ColdCry.AI
{
    [RequireComponent( typeof( Entity ) )]
    public class AIMovementBehaviour :
        MonoBehaviour,
        Utility.IObservable<AIMovementState>,
        IMemoriable<MemoryPart>
    {
        private static readonly float JUMP_Y_OFFSET = 1.2f;
        private static readonly float JUMP_TIME = 0.75f;
        private static readonly int POSITION_CHECK_PERIOD = 5;

        [Header( "Refinder" )]
        [SerializeField] [Range( 1, 500 )] private int pathRefindRepeats = 10;
        [SerializeField] [Range( 0.01f, float.MaxValue )] private float pathRefindInterval = 0.75f;

        [Header( "Utility" )]
        [SerializeField] private Utility.Logger logger;

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

            Memory = new Memorier<MemoryPart>( this );
            logger = Utility.Logger.GetInstance( gameObject );
        }

        public void Start()
        {
            //RefindPathCountdownId = TimerManager.CreateSchedule( refindInterval , this, 10 );
            RefindCountdown = ScheduledCountdown.GetInstance(
                pathRefindInterval,
                pathRefindRepeats,
                (overtime) => {
                    switch (FindPathToMovable( FollowedEntity, true )) {
                        case AIMovementResponse.TARGET_NULL:
                        case AIMovementResponse.MISSING_ENTITY_COMPONENT:
                            ClearPath();
                            ClearDummy();
                            ClearCurrentTarget();
                            break;
                        case AIMovementResponse.NO_CONTACT_AREA:
                        case AIMovementResponse.NO_PATH_TO_TARGET:
                            break;
                        case AIMovementResponse.PATH_FOUND:
                        case AIMovementResponse.TARGET_IN_SAME_AREA:
                            RefindCountdown.Stop();
                            break;
                    }
                } );
        }

        public void Update()
        {
            // mouse test
            if (Input.GetKeyDown( KeyCode.Mouse0 )) {
                Vector3 pos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                logger.Log( ReachPosition( pos ) );
            } else if (Input.GetKeyDown( KeyCode.Mouse2 )) {
                Vector3 pos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                logger.Log( ReachPosition( pos ) );
                StartPathRefind();
            } else if (Input.GetKeyDown( KeyCode.Mouse1 )) {
                logger.Log( TrackTarget( GameManager.Player ) );
            }

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

            EntityToFollow = null;

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

        private AIMovementResponse FindPathToMovable(Entity target, bool keepOldTargetIfNotFound)
        {
            Memory.Save();

            if (TargetType != AITargetType.DUMMY && !keepOldTargetIfNotFound) {
                ClearDummy();
            }

            ClearCurrentTarget();
            ClearPath();

            // FAIL CASE
            if (target == null) {
                if (keepOldTargetIfNotFound) {
                    Memory.Undo();
                }
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
                RefindCountdown.Stop();
                return AIMovementResponse.TARGET_IN_SAME_AREA;
            }

            AIPathList newPath = AIManager.FindPath( Owner, target );
            // FAIL CASE
            if (newPath == null) {
                ClearDummy();
                TargetType = AITargetType.NONE;
                return AIMovementResponse.NO_PATH_TO_TARGET;
            }

            // SUCCESS CASE
            CurrentPath = newPath;
            EntityToFollow = target;
            FollowedEntity = target;
            target.AddFollower( this );
            MovementState = AIMovementState.PATHING;
            NextNode();
            RefindCountdown.Stop();

            return AIMovementResponse.PATH_FOUND;
        }

        private AIMovementResponse FindPathToDummy(Dummy target, bool keepOldTarget)
        {
            if (keepOldTarget) {
                Memory.Save();
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
                RefindCountdown.Stop();
                return AIMovementResponse.TARGET_IN_SAME_AREA;
            }

            AIPathList newPath = AIManager.FindPath( Owner, target );
            // FAIL CASE
            if (newPath == null) {
                ClearDummy();
                TargetType = AITargetType.NONE;
                return AIMovementResponse.NO_PATH_TO_TARGET;
            }

            // SUCCESS CASE
            CurrentPath = newPath;
            EntityToFollow = target;
            FollowedEntity = target;
            target.AddFollower( this );
            MovementState = AIMovementState.PATHING;
            NextNode();
            RefindCountdown.Stop();

            return AIMovementResponse.PATH_FOUND;
        }

        public AIMovementResponse ReachPosition(Vector2 position, bool keepOldPath = false)
        {
            if (TemplateDummy == null) {
                TemplateDummy = AIManager.GetDummy( position );
            } else {
                TemplateDummy.transform.position = position;
            }
            TargetType = AITargetType.DUMMY;
            return FindPathToMovable( TemplateDummy, keepOldPath );
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
            return FindPathToMovable( entity, keepOldPath );
        }


    /*    public AIMovementResponse TrackTarget(AIReachable aIReachable, bool keepOldPath = false)
        {
            return FindPAt
        }*/

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
        public bool StartPathRefind()
        {
            RefindCountdown.Restart();
            return true;
        }

        /* public bool StartPathRefind(Entity entity)
         {

         }*/

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
                if (EntityToFollow.ContactArea != Owner.ContactArea) {
                    StartPathRefind();
                    /*
                    AIMovementResponse response = FindPath( EntityToFollowAfterPath, true );
                    switch (response) {
                        case AIMovementResponse.TARGET_NULL:
                        case AIMovementResponse.MISSING_ENTITY_COMPONENT:
                            ClearCurrentTarget();
                            ClearDummy();
                            ClearPath();
                            break;
                        case AIMovementResponse.NO_CONTACT_AREA:
                        case AIMovementResponse.NO_PATH_TO_TARGET:
                            StartPathRefind();
                            break;
                        case AIMovementResponse.PATH_FOUND:
                        case AIMovementResponse.TARGET_IN_SAME_AREA:
                            // no action needed
                            break;
                    }*/
                } else {
                    CurrentTarget = EntityToFollow.transform;
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

        public void Subscribe(Utility.IObserver<AIMovementState> observer)
        {
            if (!StateChangeObservers.Contains( observer ))
                StateChangeObservers.Add( observer );
        }

        public void Unsubscribe(Utility.IObserver<AIMovementState> observer)
        {
            StateChangeObservers.Remove( observer );
        }

        public MemoryPart SaveMemory()
        {
            MemoryPart part = new MemoryPart();
            part.Add( "TargetType", TargetType );
            part.Add( "MovementState", MovementState );
            part.Add( "TemplateDummy", TemplateDummy );
            part.Add( "FollowedEntity", FollowedEntity );
            part.Add( "EntityToFollowAfterPath", EntityToFollow );
            part.Add( "CurrentTarget", CurrentTarget );
            part.Add( "CurrentPath", CurrentPath.Clone() );
            part.Add( "CurrentNode", CurrentNode );
            return part;
        }

        public void LoadMemory(MemoryPart data)
        {
            TargetType = data.Get<AITargetType>( "TargetType" );
            MovementState = data.Get<AIMovementState>( "MovementState" );
        }

        #region Getters and Setters
        public AIPathList CurrentPath { get; private set; } = new AIPathList();
        public Entity Owner { get; private set; }
        public Entity EntityToFollow { get; private set; }
        public Entity FollowedEntity { get; private set; }
        public Transform CurrentTarget { get; private set; }
        public Dummy TemplateDummy { get; private set; }
        public ComplexNode CurrentNode { get; private set; }
        public Memorier<MemoryPart> Memory { get; private set; }
        public List<Utility.IObserver<AIMovementState>> StateChangeObservers { get; private set; } = new List<Utility.IObserver<AIMovementState>>();
        public ICountdown RefindCountdown { get; private set; }

        public AITargetType TargetType { get; private set; }
        public AIMovementState MovementState { get; private set; }

        public float RefindInterval { get => pathRefindInterval; set => pathRefindInterval = value; }
        public bool HasPath { get => !CurrentPath.IsFullyEmpty; }
        public bool PathHasEnded { get => CurrentPath.IsEmpty; }
        public bool Paused { get; set; } = false;
        public bool XPositionReached { get; set; } = false;
        public bool PositionReached { get; set; } = false;
        public int PositionCheck { get; set; } = 0;
        public int PathRefindRepeats { get => pathRefindRepeats; set => pathRefindRepeats = value; }
        public float PathRefindInterval { get => pathRefindInterval; set => pathRefindInterval = value; }
        #endregion
    }
}