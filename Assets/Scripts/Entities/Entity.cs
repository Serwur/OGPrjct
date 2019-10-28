using ColdCry.AI;
using ColdCry.AI.Movement;
using ColdCry.Core;
using ColdCry.Notifers;
using ColdCry.Utility;
using ColdCry.Utility.Time;
using System.Collections.Generic;
using UnityEngine;
using static ColdCry.Utility.Time.TimerManager;

namespace ColdCry.Objects
{
    [RequireComponent( typeof( Rigidbody ) )]
    [RequireComponent( typeof( Reachable ) )]
    public abstract class Entity : MonoBehaviour,
        IHitPointsObservable,
        ISourcePointsObservable,
        IObserver<ICountdown>,
        IContactable
    {
        [Header( "Attributes" )]
        [SerializeField] private Attribute hitPoints;
        [SerializeField] private Attribute sourcePoints;
        [SerializeField] private Attribute moveSpeed;
        [SerializeField] private Attribute jumpPower;
        [SerializeField] private Attribute damage;
        [SerializeField] private Attribute flySpeed;

        [Header( "Regeneration" )]
        [SerializeField] private float regenHitPoints = 0f;
        [SerializeField] private float regenSourcePoints = 0f;

        [Header( "Multiplayers" )]
        [Range( 0f, 100f )] private float damageTakenMult = 1f;
        [Range( 0f, 100f )] private float fallDamageMult = 1f;

        [Header( "Utility" )]
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool isDamageImmune = false;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool isBlocking = false;
        [SerializeField] private bool isPushImmune = false;
        [SerializeField] private bool isFallDamagable = true;
        [SerializeField] private bool isInAir = false;
        [SerializeField] private float minDamagableFallSpeed = 30f;

        [Header( "Dialogs" )]
        [SerializeField] private DialogueList[] dialogueLists;

        [Header( "Others" )]
        public LayerMask groundLayers;


        #region Dev Fields
        [Header( "Dev Tools" )]
        public bool anotherCollider = false;
        public string colliderChild = "";
        // For dev test, use Start on Joystick or R on keyboard to reset position and velocity
        public Vector3 startPosition;
        #endregion

        #region Protected Fields
        private List<IHitPointsObserver> hitPointsObservers;
        private List<ISourcePointsObserver> sourcePointsObservers;
        private Rigidbody rigidBody;
        private BoxCollider coll;
        [SerializeField] private Vector2 lookDirection = Vector2.right;
        [SerializeField] private float lastMinFallSpeed = float.MaxValue;
        [SerializeField] private MovementStatus movementStatus = MovementStatus.WALKING;

        protected ICountdown moveCountdown;
        #endregion

        #region Unity API
        public virtual void Awake()
        {
            // Reachable init
            Reachable = GetComponent<Reachable>();
            // Rigidbody init
            RigidBody = GetComponent<Rigidbody>();
            RigidBody.freezeRotation = true;
            startPosition = transform.position;
            HitPointsObservers = new List<IHitPointsObserver>( 1 );
            SourcePointsObservers = new List<ISourcePointsObserver>( 1 );

            // Other collider init
            // DEV TOOL TO DELETE IN FUTURE
            if (!anotherCollider) {
                Collider = GetComponent<BoxCollider>();
            } else {
                Collider = transform.Find( colliderChild ).GetComponent<BoxCollider>();
            }
        }

        public virtual void Start()
        {
            GameManager.AddEntity( this );
            UpdateAttributes();
            HitPoints.Current = HitPoints.Max;
            SourcePoints.Current = SourcePoints.Max;
            MoveSpeed.Current = MoveSpeed.Max;
            JumpPower.Current = JumpPower.Max;
            Damage.Current = Damage.Max;
            FlySpeed.Current = FlySpeed.Max;

            moveCountdown = Countdown.GetInstance();
            moveCountdown.SetAction( (overtime) => { canMove = true; } );
        }

        public virtual void FixedUpdate()
        {
            if (LastMinFallSpeed > RigidBody.velocity.y) {
                LastMinFallSpeed = RigidBody.velocity.y;
            }
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
            if (IsTouchingGround()) {
                Fallen( LastMinFallSpeed * -1 );
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Do zaimplementowania skrypt ruchu gracza.
        /// </summary>
        public void Move(float x)
        {
            if (MovementStatus == MovementStatus.WALKING)
                Move( MoveSpeed.Current, x );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( FlySpeed.Current, x );
        }

        public void Move(Vector2 destination)
        {
            if (MovementStatus == MovementStatus.WALKING)
                Move( moveSpeed.Current, destination );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( flySpeed.Current, destination );
        }

        public void Move(float moveSpeed, Vector2 destination)
        {
            Move( moveSpeed, transform.position.x - destination.x );
        }

        public void Move(Entity entity)
        {
            if (MovementStatus == MovementStatus.WALKING)
                Move( moveSpeed.Current, entity );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( flySpeed.Current, entity );
        }

        public void Move(float moveSpeed, Entity entity)
        {
            Move( moveSpeed, entity.transform.position.x - transform.position.x );
        }

        public void Move(Transform _transform)
        {
            if (MovementStatus == MovementStatus.WALKING)
                Move( moveSpeed.Current, _transform );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( flySpeed.Current, _transform );
        }

        public void Move(float moveSpeed, Transform _transform)
        {
            Move( moveSpeed, _transform.position.x - transform.position.x );
        }

        public void Move(float moveSpeed, float x)
        {
            if (CanMove && moveSpeed > 0) {
                LookDirection = x > 0 ? Vector2.right : Vector2.left;
                RigidBody.velocity = new Vector3( 0, RigidBody.velocity.y );
                transform.Translate( new Vector3( LookDirection.x * moveSpeed * Time.deltaTime, 0 ), Space.World );
                transform.rotation = Quaternion.LookRotation( new Vector3( 0, 0, LookDirection.x ), transform.up );
                OnMove( moveSpeed, LookDirection );
            }
        }

        public void Jump(float jumpPower)
        {
            if (jumpPower > 0) {
                RigidBody.velocity = new Vector2( RigidBody.velocity.x, jumpPower );
                LastMinFallSpeed = 0;
                MovementStatus = MovementStatus.JUMPING;
                IsInAir = true;
                OnJump( jumpPower );
            }
        }

        /// <summary>
        /// Zadaje obrażenia jednostce.
        /// </summary>
        /// <param name="attack">Informacje o wykonywanym ataku</param>
        /// <returns>Zwraca TRUE, jeżeli jednostka otrzymała jakiekolwiek obrażenia w przeciwnym wypadku zwraca FALSE</returns>
        public bool TakeDamage(Attack attack)
        {
            // SPRAWDZA CZY JEDNOSTKA NIE ZABLOKOWAŁA ATAKU
            bool blocked = ShouldBlockAttack( attack );
            // JEŻELI JEDNOSTKA NIE JEST ODPORNA NA ODEPCHNIĘCIA I MOC
            // ODEPCHNIĘCIA JEST WIĘKSZA OD 0 TO JĄ ODPYCHA
            if (!IsPushImmune && attack.PushPower > 0f) {
                PushOff( attack, blocked );
            }
            // JEŻELI JEDNOSTKA JEST NIETYKALNA, ZABLOKOWAŁA ATAK
            // ORAZ ZADAWANE OBRAŻENIA SĄ WIĘKSZE NIŻ 0 TO TRACI PUNKTY ŻYCIA
            if (!( IsDamageImmune || blocked ) && attack.Damage > 0f) {
                if (attack.PercenteDamage) {
                    HitPoints.Current -= HitPoints.Max * attack.Damage;
                } else {
                    HitPoints.Current -= attack.Damage;
                }
                OnDamageTook( attack );
                if (HitPoints.Current <= 0f)
                    Die();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Metoda lecząca jednostkę o podane punkty zdrowia. Nadpisać metodę w klasach pochodnych, jeżeli istnieje taka potrzeba.
        /// </summary>
        /// <param name="heal">Ilość punktów zdrowia do uleczenia</param>
        /// <returns>TRUE jeżeli zwiększono ilość punktów zdrowia, w przeciwnym wypadku FALSE</returns>
        public bool Heal(float heal)
        {
            float nextHitPoints = HitPoints.Current + heal;
            if (nextHitPoints > HitPoints.Max)
                HitPoints.Current = HitPoints.Max;
            else
                HitPoints.Current = nextHitPoints;
            OnBeingHealed( heal );
            return true;
        }

        /// <summary>
        /// <br>Zabija jednostkę wykonując przy tym zaimplementowane działania np. wykonywanie animacji śmierci, dodanie</br>
        /// <br>punktów graczom itp. itd.</br>
        /// </summary>
        public void Die()
        {
            RigidBody.freezeRotation = false;
            RigidBody.AddForce( new Vector3( UnityEngine.Random.Range( -14, 14 ), UnityEngine.Random.Range( -5, 25 ) ), ForceMode.VelocityChange );
            RigidBody.AddTorque( new Vector3( UnityEngine.Random.Range( 15, 35 ), UnityEngine.Random.Range( -25, 25 ), UnityEngine.Random.Range( -25, 25 ) ), ForceMode.VelocityChange );
            IsDead = true;
            OnDie();
        }

        public virtual void PushOff(Attack attack, bool blocked = false)
        {
            PushOff( attack.PushPower, attack.PushDirection, attack.PushDisableTime, blocked );
        }

        /// <summary>
        /// Odrzuca jednostkę nadając jej pewną prędkość. Można przy tym wyłączyć możliwość ruchu
        /// na pewną ilość sekund.
        /// </summary>
        /// <param name="pushPower">Siła odrzutu</param>
        /// <param name="direction">Kierunek odrzutu</param>
        /// <param name="pushDisableTime">Czas wstrzymania możliwości ruchu, jeżeli 0 lub mniej to ruch
        /// nie zostaje wyłączony</param>
        public virtual void PushOff(float pushPower, Vector3 direction, float pushDisableTime, bool blocked = false)
        {
            // JEŻELI PUSH DISABLE TIME > 0 I JEDNOSTKA NIE BLOKUJE TO UNIEAKTYWNIA RUCH
            if (pushDisableTime > 0 && !blocked) {
                CanMove = false;
                if (moveCountdown.Remaing < pushDisableTime) {
                    // USTAWIA TIMER PO KTÓRYM JEDNOSTKA ODZYSKUJE MOŻLIWOŚĆ RUCHU
                    moveCountdown.Restart( pushDisableTime );
                }
            }
            // NADAJE PRĘDKOŚĆ
            RigidBody.velocity = direction * pushPower;
            OnPushedOff( pushPower, direction, pushDisableTime );
        }

        /// <summary>
        /// <br>Metoda wywoływana co sekundę w celu regeneracji zycia oraz zasobów.</br>
        /// <br>Powinna być wywołana w jednej klasie, która będzie mieć dostep do wszystkich jednostek np. GameMaster.</br>
        /// </summary>
        public void Regenerate()
        {
            float nextHitPoints = HitPoints.Current + RegenHitPoints;
            if (nextHitPoints > HitPoints.Max)
                HitPoints.Current = HitPoints.Max;
            else
                HitPoints.Current = nextHitPoints;

            float nextSourcePoints = SourcePoints.Current + RegenSourcePoints;
            if (nextSourcePoints > SourcePoints.Max)
                SourcePoints.Current = HitPoints.Max;
            else
                SourcePoints.Current = nextSourcePoints;

            OnRegenerate();
        }

        /// <summary>
        /// Checks if attack should be blocked
        /// </summary>
        /// <returns><code>TRUE</code> if attack should be blocked, otherwise <code>FALSE</code></returns>
        public virtual bool ShouldBlockAttack(Attack attack)
        {
            if (!IsBlocking && attack.AttackDirection == 0)
                return false;
            return attack.AttackDirection != LookDirection.x;
        }

        public void Fallen(float speedWhenFalling)
        {
            isInAir = false;
            MovementStatus = MovementStatus.WALKING;
            if (IsFallDamagable && speedWhenFalling > minDamagableFallSpeed) {
                TakeDamage(
                    new Attack(
                        ( speedWhenFalling - minDamagableFallSpeed ) / minDamagableFallSpeed,
                        true ) );
            }
            LastMinFallSpeed = float.MaxValue;
            OnFallen( speedWhenFalling );
        }

        /// <summary>
        /// <br>Aktualizuje statystyki jednostki takie jak maksymalna ilość zdrowia, maksymalna ilość zasobów, atak, pancerz itp. itd.</br>
        /// <br>Metoda powinna być wywoływana w momencie zmiany którejś ze statystyk zwiększających lub zmniejsząjących obrażenia, życie maksymalne itp..</br>
        /// </summary>
        public virtual void UpdateAttributes()
        {
            HitPoints.UpdateAttribute();
            SourcePoints.UpdateAttribute();
            MoveSpeed.UpdateAttribute();
            JumpPower.UpdateAttribute();
            Damage.UpdateAttribute();
        }

        /// <summary>
        /// Checks if entity is touching ground
        /// </summary>
        /// <returns>TRUE if character is touching ground, otherwise FALSE</returns>
        public virtual bool IsTouchingGround()
        {
            return Physics.CheckBox( new Vector3( Collider.bounds.center.x, Collider.bounds.min.y, Collider.bounds.center.z ),
                new Vector3( Collider.bounds.extents.x * 0.85f, Collider.bounds.extents.y * 0.1f, Collider.bounds.extents.z * 0.85f ),
                Collider.transform.rotation, groundLayers );
        }

        /// <summary>
        /// <br>Zamysł tej metody jest taki, że resetuje stan gracza ustawiajac jego hp i zasoby do 100%,</br>
        /// <br>usuwając wszystkie czasowe oraz pasywne buffy/debuffy (wliczajac w to wzmocnienia, stuny itd. itp)</br>
        /// </summary>
        public virtual void ResetUnit()
        {
            RigidBody.velocity = Vector3.zero;
            transform.position = startPosition;
            IsBlocking = false;
            CanMove = true;
            IsDamageImmune = false;
            LastMinFallSpeed = 0;
            IsDead = false;
            RigidBody.freezeRotation = true;
            RigidBody.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler( Vector3.zero );
            HitPoints.Current = HitPoints.Max;
        }

        public virtual void OnContactAreaEnter(ContactArea contactArea)
        {

        }

        public virtual void OnContactAreaExit(ContactArea contactArea)
        {

        }

        public virtual void AddHitPointsObserver(IHitPointsObserver observer)
        {

        }

        public virtual void RemoveHitPointsObserver(IHitPointsObserver observer)
        {

        }

        public virtual void AddSourcePointsObserver(ISourcePointsObserver observer)
        {
        }

        public virtual void RemoveSourcePointsObserver(ISourcePointsObserver observer)
        {

        }

        public virtual void Notify(ICountdown notifier)
        {

        }
        #endregion

        #region Abstract
        public abstract void OnMove(float moveSpeed, Vector2 direction);
        public abstract void OnJump(float jumpPower);
        public abstract void OnFallen(float speedWhenFalling);
        public abstract void OnDamageTook(Attack attack);
        public abstract void OnRegenerate();
        public abstract void OnPushedOff(float pushPower, Vector3 direction, float disableTime);
        public abstract void OnBeingHealed(float healedHp);
        public abstract void OnDie();
        public abstract void DrawGizmos();
        #endregion

        #region Setter And Getters
        public Reachable Reachable { get; private set; }
        public Vector3 LookRotation { get => new Vector3( LookDirection.x, 0 ); }
        public Vector2 LookDirection { get => lookDirection; set => lookDirection = value; }
        public Attribute HitPoints { get => hitPoints; set => hitPoints = value; }
        public Attribute SourcePoints { get => sourcePoints; set => sourcePoints = value; }
        public Attribute MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public Attribute JumpPower { get => jumpPower; set => jumpPower = value; }
        public Attribute Damage { get => damage; set => damage = value; }
        public Attribute FlySpeed { get => flySpeed; set => flySpeed = value; }
        public DialogueList[] DialogueLists { get => dialogueLists; set => dialogueLists = value; }
        public List<IHitPointsObserver> HitPointsObservers { get => hitPointsObservers; set => hitPointsObservers = value; }
        public List<ISourcePointsObserver> SourcePointsObservers { get => sourcePointsObservers; set => sourcePointsObservers = value; }
        public Rigidbody RigidBody { get => rigidBody; private set => rigidBody = value; }
        public BoxCollider Collider { get => coll; private set => coll = value; }
        public MovementStatus MovementStatus { get => movementStatus; set => movementStatus = value; }

        public float RegenHitPoints { get => regenHitPoints; set => regenHitPoints = value; }
        public float RegenSourcePoints { get => regenSourcePoints; set => regenSourcePoints = value; }
        public float DamageTakenMult { get => damageTakenMult; set => damageTakenMult = value; }
        public float FallDamageMult { get => fallDamageMult; set => fallDamageMult = value; }
        public float MinDamagableFallSpeed { get => minDamagableFallSpeed; set => minDamagableFallSpeed = value; }
        public float LastMinFallSpeed { get => lastMinFallSpeed; set => lastMinFallSpeed = value; }

        public bool IsDead { get => isDead; set => isDead = value; }
        public bool CanMove { get => canMove; set => canMove = value; }
        public bool IsDamageImmune { get => isDamageImmune; set => isDamageImmune = value; }
        public bool IsPaused { get => isPaused; set => isPaused = value; }
        public bool IsBlocking { get => isBlocking; set => isBlocking = value; }
        public bool IsPushImmune { get => isPushImmune; set => isPushImmune = value; }
        public bool IsFallDamagable { get => isFallDamagable; set => isFallDamagable = value; }
        public bool IsInAir { get => isInAir; set => isInAir = value; }
        #endregion
    }
}