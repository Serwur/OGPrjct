using ColdCry.Core;
using ColdCry.Objects;
using ColdCry.Utility.Patterns.Memory;
using ColdCry.Utility.Time;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ColdCry.Utility.Time.TimerManager;

namespace ColdCry.AI.Movement
{
    [RequireComponent( typeof( Reachable ) )]
    [RequireComponent( typeof( Entity ) )]
    public class AIMovementBehaviour :
        MonoBehaviour,
        Utility.IObservable<AIMovementState>,
        IMemoriable<MemoryPart>,
        IContactable
    {
        private static readonly float JUMP_Y_OFFSET = 1.2f;
        private static readonly float JUMP_TIME = 0.75f;
        private static readonly int POSITION_CHECK_PERIOD = 5;

        [Header( "Custom Behaviour" )]
        [SerializeField] private bool waitForTargetWhenNotInCA = false;
        [SerializeField] private bool keepOldTargetIfNotFound = false;

        [SerializeField] private float distForNextNode = 0.45f;
        [SerializeField] private float distToStopFollow = 0.33f;

        [Header( "Stuck: not reached node" )]
        [SerializeField] private float nrn_timeToUnstuck = 1.3f;
        [SerializeField] private float nrn_distanceToUnstuck = 0.4f;

        [Header( "Refinder" )]
        [SerializeField] private bool refindWhenNotFound = true;
        [SerializeField] private bool refindWhenTargetNotInCA = true;
        [SerializeField] private bool refindWhenOwnerNotInCA = true;

        [SerializeField] [Range( 1, 500 )] private int pathRefindRepeats = 10;
        [SerializeField] [Range( 0.01f, float.MaxValue )] private float pathRefindInterval = 0.75f;

        [Header( "Utility" )]
        [SerializeField] private AIMovementState movementState;
        [SerializeField] private Utility.Logger logger;
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool testingMode = false;

        private bool awaken = false;
        private bool refindOnNextNode = false;

        #region Unity API
        public void Awake()
        {
            ReachableOwner = GetComponent<Reachable>();
            Owner = GetComponent<Entity>();
            Memory = new Memorier<MemoryPart>( this );
            logger = Utility.Logger.GetInstance( gameObject );
        }

        public void Start()
        {
            RefindCountdown = ScheduledCountdown.GetInstance(
                pathRefindInterval,
                pathRefindRepeats,
                (overtime) => {
                    AIMovementResponse response = ResponseRefindBehaviour( RefindPath() );
                    logger.Log( "Refind response: " + response + ", repeats left: " + ( RefindCountdown as ScheduledCountdown ).CurrentRepeat );
                } );

            NotReachedNodeCountdown_Stuck = Countdown.GetInstance(
                nrn_timeToUnstuck,
                (overtime) => {
                   if ( MovementState == AIMovementState.PATHING ) {
                        // implement timer to get variables to be able to check them later
                        // additionally implemented to get Action parameter without variable
                   }
                }
                );

            OwnReachable = AIManager.GetReachable();
        }

        public void Update()
        {
            // mouse test
            if (testingMode) {
                if (Input.GetKeyDown( KeyCode.Mouse0 )) {
                    Vector3 pos = Camera.main.ScreenToWorldPoint( Input.mousePosition + new Vector3( 0, 0, 5f ) );
                    /*float cos = Mathf.Cos( Mathf.Deg2Rad * Camera.main.fieldOfView );
                    if (cos != 0f) {
                        pos /= cos;
                        logger.Log( pos );
                        ReachPosition( pos );
                    }*/
                    ReachPosition( pos );
                } else if (Input.GetKeyDown( KeyCode.Mouse1 )) {
                    logger.Log( FollowTarget( GameManager.Player.GetComponent<Reachable>() ) );
                } else if (Input.GetKey( KeyCode.Mouse2 )) {
                    Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
                    if (Physics.Raycast( ray, out RaycastHit hit )) {
                        Reachable reachable = hit.collider.GetComponent<Reachable>();
                        if (reachable != null) {
                            logger.Log( FollowTarget( reachable ) );
                        } else {
                            Stop();
                        }
                    }
                }
            }

            if (Owner.CanMove && !Paused)
                switch (MovementState) {
                    case AIMovementState.NONE:
                        break;
                    case AIMovementState.WAITING:
                        // just wait and do nothing
                        break;
                    case AIMovementState.REFINDING:
                        break;
                    case AIMovementState.PATHING:
                        PathUpdate();
                        break;
                    case AIMovementState.FOLLOWING:
                        FollowUpdate();
                        break;
                    case AIMovementState.REACHED:
                        ReachedUpdate();
                        // wait till target will be out of range
                        break;
                }
        }

        public void OnEnable()
        {
            if (awaken)
                OwnReachable = AIManager.GetReachable();
            awaken = true;
        }

        public void OnDisable()
        {
            if (OwnReachable != null)
                AIManager.ReturnReachable( OwnReachable );
            Stop();
            Owner.ResetUnit();
        }

        public void OnDestroy()
        {
            if (OwnReachable != null)
                AIManager.ReturnReachable( OwnReachable );
            Stop();
            Owner.ResetUnit();
        }
        #endregion

        #region StateUpdates
        private void PathUpdate()
        {
            if (Target.Current != null) {
                PositionCheck++;
                if (PositionCheck >= POSITION_CHECK_PERIOD) {
                    PositionCheck = 0;
                    if (Vector2.Distance( Target.Current.position, transform.position ) <= distForNextNode) {
                        NextNode();
                        return;
                    }
                }

                // when entity in in air and x position has been reached we can't move him,
                // otherwise we move him anyway
                if (Owner.IsInAir) {
                    if (Mathf.Abs( Target.Current.position.x - transform.position.x ) >= distForNextNode) {
                        Owner.Move( Target.Current );
                    }
                } else {
                    Owner.Move( Target.Current );
                }
            }
        }

        private void FollowUpdate()
        {
            if (Target.Current != null) {
                if (Target.AfterPath.transform != Target.Current) {
                    AIMovementResponse response = FindPath( Target.AfterPath, true, Target.Type );
                    ResponseBehaviour( response );
                    return;
                }
                if (Mathf.Abs( Target.Current.position.x - transform.position.x ) <= distToStopFollow) {
                    switch (Target.Type) {
                        case AITargetType.STEADY:
                            Stop();
                            return;
                        case AITargetType.MOVABLE:
                            MovementState = AIMovementState.REACHED;
                            return;
                    }
                }
                Owner.Move( Target.Current );
            }
        }

        private void ReachedUpdate()
        {
            if (Target.Current != Target.AfterPath.transform ||
                Mathf.Abs( Target.AfterPath.transform.position.x - transform.position.x ) >= distToStopFollow) {
                AIMovementResponse response = RefindPath();
                ResponseBehaviour( response );
            }
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
            //RefindCountdown.Pause();
        }

        /// <summary>
        /// Stops all actions, deletes references and back to SLEEP state
        /// </summary>
        public void Stop()
        {
            ClearTarget();
            RefindCountdown.Stop();
            MovementState = AIMovementState.NONE;
            Paused = false;
            refindOnNextNode = false;
        }

        private void ClearTarget()
        {
            if (Target.AfterPath != null) {
                Target.AfterPath.RemoveAIFollower( this );
            }
            Target = AIMovementTarget.Empty;
        }

        public AIMovementResponse ReachPosition(Vector2 position, bool keepOldPath = true)
        {
            if (Owner.IsDead) {
                return ResponseBehaviour( AIMovementResponse.OWNER_IS_DEAD );
            }
            OwnReachable.transform.position = position;
            return ResponseBehaviour( FindPath( OwnReachable, keepOldPath, AITargetType.STEADY ) );
        }

        public AIMovementResponse ReachPosition(Transform transform, bool keepOldPath = true)
        {
            return ReachPosition( transform.position, keepOldPath );
        }

        public AIMovementResponse ReachPosition(Reachable reachable, bool keepOldPath = true)
        {
            return ReachPosition( reachable.transform.position, keepOldPath );
        }

        public AIMovementResponse ReachPosition(Entity entity, bool keepOldPath)
        {
            // response behaviour is handled in up function
            return ReachPosition( entity.transform.position, keepOldPath );
        }

        public AIMovementResponse FollowTarget(Transform transform, bool keepOldPath = true)
        {
            if (transform == null) {
                return ResponseBehaviour( AIMovementResponse.TARGET_NULL );
            }

            Reachable reachable = transform.GetComponent<Reachable>();

            if (reachable == null) {
                return ResponseBehaviour( AIMovementResponse.TARGET_MISSING_REACHABLE_COMPONENT );
            }

            return ResponseBehaviour( FollowTarget( reachable, keepOldPath ) );
        }

        public AIMovementResponse FollowTarget(Entity entity, bool keepOldPath = true)
        {
            Reachable reachable = entity.GetComponent<Reachable>();

            if (reachable == null) {
                return ResponseBehaviour( AIMovementResponse.TARGET_MISSING_REACHABLE_COMPONENT );
            }

            return ResponseBehaviour( FollowTarget( reachable, keepOldPath ) );
        }

        public AIMovementResponse FollowTarget(Reachable reachable, bool keepOldPath = true)
        {
            if (Owner.IsDead) {
                return ResponseBehaviour( AIMovementResponse.OWNER_IS_DEAD );
            }
            return ResponseBehaviour( FindPath( reachable, keepOldPath, AITargetType.MOVABLE ) );
        }

        private AIMovementResponse FindPath(Reachable target,
            bool keepOldTargetIfNotFound,
            AITargetType targetType)
        {
            if (Target.AfterPath != null) {
                Memory.Save();
            }

            Target = new AIMovementTarget(
                target.transform,
                AIPathList.Empty,
                target,
                targetType,
                false
                );

            // FAIL CASE
            if (target == null) {
                return AIMovementResponse.TARGET_NULL;
            }

            if (target.ContactArea == null) {
                return AIMovementResponse.TARGET_NOT_IN_CONTACT_AREA;
            }

            // FAIL CASE
            if (ReachableOwner.ContactArea == null) {
                return AIMovementResponse.OWNER_NOT_IN_CONTACT_AREA;
            }

            // SUCCESS CASE
            if (target.ContactArea == ReachableOwner.ContactArea) {
                RefindCountdown.Stop();
                MovementState = AIMovementState.FOLLOWING;
                target.AddAIFollower( this );
                Target = new AIMovementTarget(
                    target.transform,
                    AIPathList.Empty,
                    target,
                    targetType,
                    false
                    );
                return AIMovementResponse.TARGET_IN_SAME_AREA;
            }

            AIPathList newPath = AIManager.FindPath( ReachableOwner, target );
            // FAIL CASE
            if (newPath == null) {
                return AIMovementResponse.NO_PATH_TO_TARGET;
            }

            // SUCCESS CASE
            RefindCountdown.Stop();
            MovementState = AIMovementState.PATHING;
            target.AddAIFollower( this );
            Target = new AIMovementTarget(
                null,
                newPath,
                target,
                targetType,
                false
                );
            NextNode();

            return AIMovementResponse.PATH_FOUND;
        }

        private AIMovementResponse RefindPath()
        {
            // FAIL CASE
            if (ReachableOwner.ContactArea == null) {
                return AIMovementResponse.OWNER_NOT_IN_CONTACT_AREA;
            }

            // FAIL CASE 
            if (Target.AfterPath == null) {
                return AIMovementResponse.TARGET_NULL;
            }

            // FAIL CASE
            if (Target.AfterPath.ContactArea == null) {
                return AIMovementResponse.TARGET_NOT_IN_CONTACT_AREA;
            }

            /*
             * Elo kurwa, tutaj trzeba dodać pozostałe przypadki: zostały chyba tylko udane,
             * a dokładniej gdy jest ta sama area. Potem należy obsłużyć zwracane responsy
             * dla refinda. Po tym wszystkim sprawdzić pozostałe bugi, a następnie ogarnąć
             * aby timer poprawnie się wykonywał. To jest priorytet. Po tym wszystkim
             * wykonać potrzebne testy, jeżeli wszystko śmiga to commit i w końcu wszystko poprawnie działa.
             * Następne rzeczy wtedy dopiero wprowadzać jako feature!
             * 
            */

            if (Target.AfterPath.ContactArea == ReachableOwner.ContactArea) {
                return AIMovementResponse.TARGET_IN_SAME_AREA;
            }

            AIPathList newPath = AIManager.FindPath( ReachableOwner, Target.AfterPath );
            // FAIL CASE
            if (newPath == null) {
                return AIMovementResponse.NO_PATH_TO_TARGET;
            }

            // SUCCESS CASE
            Target.AfterPath.AddAIFollower( this );
            Target = new AIMovementTarget(
                null,
                newPath,
                Target.AfterPath,
                Target.Type,
                Target.KeepInMemory
                );
            NextNode();
            return AIMovementResponse.PATH_FOUND;
        }

        private AIMovementResponse ResponseBehaviour(AIMovementResponse response)
        {
            bool shouldClear = false;

            switch (response) {
                case AIMovementResponse.TARGET_NULL:
                case AIMovementResponse.TARGET_MISSING_REACHABLE_COMPONENT:
                    if (keepOldTargetIfNotFound) {
                        Memory.Undo();
                    } else {
                        shouldClear = true;
                    }
                    break;
                case AIMovementResponse.OWNER_NOT_IN_CONTACT_AREA:
                    if (refindWhenOwnerNotInCA) {
                        StartPathRefind();
                    } else {
                        shouldClear = true;
                    }
                    break;
                case AIMovementResponse.NO_PATH_TO_TARGET:
                    if (refindWhenNotFound) {
                        StartPathRefind();
                    } else {
                        shouldClear = true;
                    }
                    break;
                case AIMovementResponse.TARGET_NOT_IN_CONTACT_AREA:
                    if (refindWhenTargetNotInCA) {
                        StartPathRefind();
                    } else {
                        shouldClear = true;
                    }
                    break;
                case AIMovementResponse.TARGET_IN_SAME_AREA:
                    MovementState = AIMovementState.FOLLOWING;
                    break;
                case AIMovementResponse.PATH_FOUND:
                    MovementState = AIMovementState.PATHING;
                    break;
            }

            if (shouldClear) {
                MovementState = AIMovementState.NONE;
                ClearTarget();
            }

            return response;
        }

        private AIMovementResponse ResponseRefindBehaviour(AIMovementResponse response)
        {
            switch (response) {
                case AIMovementResponse.TARGET_NULL: // stops all actions
                case AIMovementResponse.TARGET_MISSING_REACHABLE_COMPONENT:
                case AIMovementResponse.OWNER_IS_DEAD:
                    Stop();
                    break;
                case AIMovementResponse.TARGET_NOT_IN_CONTACT_AREA: // just wait till refind counter ends
                case AIMovementResponse.NO_PATH_TO_TARGET:
                case AIMovementResponse.OWNER_NOT_IN_CONTACT_AREA:
                    if (RefindCountdown.HasEnded()) {
                        Stop();
                    }
                    break;
                case AIMovementResponse.TARGET_IN_SAME_AREA:
                    MovementState = AIMovementState.FOLLOWING;
                    RefindCountdown.Stop();
                    break;
                case AIMovementResponse.PATH_FOUND:
                    MovementState = AIMovementState.PATHING;
                    RefindCountdown.Stop();
                    break;

            }
            return response;
        }

        /// <summary>
        /// Start an interval timer which calculate the shortest path to Current target.
        /// It stops when the path is found or if it exceeds given break time.
        /// If Current target is null then it won't start.
        /// </summary>
        /// <returns></returns>
        public bool StartPathRefind()
        {
            MovementState = AIMovementState.REFINDING;
            RefindCountdown.Restart();
            return true;
        }

        /// <summary>
        /// Gives next node to follow in AI path
        /// </summary>
        private void NextNode()
        {
            if (refindOnNextNode) {
                refindOnNextNode = false;
                /*   if (Target.AfterPath.ContactArea != null && !Target.AfterPath.ContactArea.Contains( Target.Path.Last.Node )) {
                       AIMovementResponse response = RefindPath();
                       ResponseBehaviour( response );
                       return;
                   } else if (Target.AfterPath.ContactArea == null) {
                       AIMovementResponse response = RefindPath();
                       ResponseBehaviour( response );
                       return;
                   }*/
                AIMovementResponse response = RefindPath();
                ResponseBehaviour( response );
                return;
            }

            if (!Target.Path.IsEmpty) {
                PositionReached = false;
                XPositionReached = false;
                Target.Path.Next();
                Target.Current = Target.Path.Current.Node.transform;
                switch (Target.Path.Current.Action) {
                    case AIAction.WALK:
                        break;
                    case AIAction.JUMP:
                        // Sets jump power that depdens on height that must be reached
                        float heigth = Target.Current.position.y - transform.position.y;
                        float jumpPower = heigth / JUMP_TIME - Physics.gravity.y / 2f * JUMP_TIME;
                        if (jumpPower < 0) {
                            jumpPower = 0;
                        }
                        JumpTo( jumpPower, Target.Current.position );
                        break;
                }
            } else {
                if (Target.AfterPath.ContactArea != ReachableOwner.ContactArea) {
                    StartPathRefind();
                } else {
                    Target.Current = Target.AfterPath.transform;
                    MovementState = AIMovementState.FOLLOWING;
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

        public void OnContactAreaEnter(ContactArea contactArea)
        {
            if (MovementState == AIMovementState.PATHING) {
                // przypadek jeżeli AI spadł/wypadł z area i kolejna area na jaką trafia nie jest
                // docelową to trzeba trasę na nowo wyszukać
                if (OwnReachable.ContactArea == null && !contactArea.Contains( Target.Path.CurrentNode )) {
                    ResponseBehaviour( FindPath( Target.AfterPath, true, Target.Type ) );
                }
            } else if (movementState == AIMovementState.FOLLOWING) {
                if (OwnReachable.ContactArea == null || OwnReachable.ContactArea != Target.AfterPath.ContactArea) {
                    ResponseBehaviour( FindPath( Target.AfterPath, true, Target.Type ) );
                }
            }
        }

        public void OnContactAreaExit(ContactArea contactArea)
        {

        }

        public void OnFollowedObjectAreaEnter(ContactArea contactArea, Reachable reachable)
        {
            if ( MovementState == AIMovementState.FOLLOWING ) {
                if ( contactArea.Contains(Target.Path.CurrentNode) ) {
                    return;
                }
            }

            if (Owner.MovementStatus == MovementStatus.JUMPING) {
                refindOnNextNode = true;
            } else {
                AIMovementResponse response = RefindPath();
                ResponseBehaviour( response );
            }
        }

        public void OnFollowedObjectAreaExit(ContactArea contactArea, Reachable reachable)
        {
            if (waitForTargetWhenNotInCA && movementState != AIMovementState.PATHING) {
                MovementState = AIMovementState.WAITING;
            }
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
            part.Add( "MovementState", MovementState );
            part.Add( "Target", Target.Clone() );
            return part;
        }

        public void LoadMemory(MemoryPart data)
        {
            MovementState = data.Get<AIMovementState>( "MovementState" );
            Target = data.Get<AIMovementTarget>( "Target" );
        }

        public void OnDrawGizmos()
        {
            if (drawGizmos && Target.Current != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine( transform.position, Target.Current.position );
            }
        }

        #region Getters and Setters
        public Entity Owner { get; private set; } // without it it won't work
        public Reachable ReachableOwner { get; private set; } // without it it won't work
        public Reachable OwnReachable { get; private set; } // used for reaching position
        public Memorier<MemoryPart> Memory { get; private set; }
        public List<Utility.IObserver<AIMovementState>> StateChangeObservers { get; private set; } = new List<Utility.IObserver<AIMovementState>>();
        public AIMovementState MovementState { get => movementState; private set => movementState = value; }
        private AIMovementTarget Target = AIMovementTarget.Empty;

        public ICountdown RefindCountdown { get; private set; }
        public ICountdown NotReachedNodeCountdown_Stuck { get; private set; }

        public float RefindInterval { get => pathRefindInterval; set => pathRefindInterval = value; }
        //  public bool IsReaching { get => Target.TargetAfterPath != null; }
        public bool Paused { get; set; } = false;
        public bool XPositionReached { get; set; } = false;
        public bool PositionReached { get; set; } = false;
        public int PositionCheck { get; set; } = 0;
        public int PathRefindRepeats { get => pathRefindRepeats; set => pathRefindRepeats = value; }
        public float PathRefindInterval { get => pathRefindInterval; set => pathRefindInterval = value; }
        #endregion

        private struct AIMovementTarget : ICloneable
        {
            private static readonly AIMovementTarget RD_EMPTY;

            static AIMovementTarget()
            {
                RD_EMPTY = new AIMovementTarget(
                    null,
                    AIPathList.Empty,
                    null,
                    AITargetType.NONE,
                    false
                    );
            }

            public Transform Current { get; set; }
            public AIPathList Path { get; set; }
            public Reachable AfterPath { get; set; }
            public AITargetType Type { get; set; }
            public bool KeepInMemory { get; set; }

            public AIMovementTarget(Transform current, AIPathList path, Reachable afterPath, AITargetType type, bool keepInMemory)
            {
                Current = current;
                Path = path;
                AfterPath = afterPath;
                Type = type;
                this.KeepInMemory = keepInMemory;
            }

            public object Clone()
            {
                AIMovementTarget clone = new AIMovementTarget(
                    Current,
                    Path,
                    AfterPath,
                    Type,
                    KeepInMemory
                );
                return clone;
            }

            public static AIMovementTarget Empty { get; }
        }

        // to implemented later
        public class AIPathBuilder
        {

            public AIPathBuilder RefindWhenNotFound()
            {
                return this;
            }

            public AIPathBuilder RefindWhenTargetNotInCA()
            {
                return this;
            }

            public AIPathBuilder RefindWhenOwnerNotInCA()
            {
                return this;
            }

            public AIPathBuilder Reach()
            {
                return this;
            }

            public AIPathBuilder Follow()
            {
                return this;
            }

            public void Find()
            {

            }
        }

    }
}