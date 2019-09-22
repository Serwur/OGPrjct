using ColdCry.AI;
using ColdCry.Core;
using ColdCry.Notifers;
using ColdCry.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.Objects
{
    [RequireComponent( typeof( Rigidbody ) )]
    public abstract class Entity : MonoBehaviour, IOnCountdownEnd, IHitPointsObservable, ISourcePointsObservable
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
        [Range( 0.01f, 10f )] private float damageTakenMult = 1f;
        [Range( 0.01f, 10f )] private float fallDamageMult = 1f;

        [Header( "Utility" )]
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool isInviolability = false;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool isBlocking = false;
        [SerializeField] private bool isPushImmune = false;
        [SerializeField] private bool isFallDamagable = true;
        [SerializeField] private bool isInAir = false;

        [Header( "Dialogs" )]
        [SerializeField] private DialogueList[] dialogueLists;

        [Header( "Others" )]
        public LayerMask groundLayers;
        [Range( 0.05f, 1.5f )] private float pathRefindTimer = 0.6f;
        [SerializeField] private float minDamagableFallSpeed = 30f;

        #region Dev Fields
        [Header( "Dev Tools" )]
        public bool anotherCollider = false;
        public string colliderChild = "";
        // For dev test, use Start on Joystick or R on keyboard to reset position and velocity
        public Vector3 startPosition;
        #endregion

        #region Protected Fields
        private HashSet<AIBehaviour> aiFollowers = new HashSet<AIBehaviour>();
        private List<IHitPointsObserver> hitPointsObservers;
        private List<ISourcePointsObserver> sourcePointsObservers;
        private Rigidbody rb;
        private BoxCollider coll;
        private ContactArea contactArea;
        private Vector2 lookDirection = Vector2.right;
        private float lastMinFallSpeed = float.MaxValue;
        private MovementStatus movementStatus = MovementStatus.WALKING;

        protected long moveCountdownId;
        #endregion

        #region Unity API
        public virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Rb.freezeRotation = true;
            startPosition = transform.position;
            HitPointsObservers = new List<IHitPointsObserver>( 1 );
            SourcePointsObservers = new List<ISourcePointsObserver>( 1 );

            // DEV TOOL TO DELETE IN FUTURE
            if (!anotherCollider) {
                Coll = GetComponent<BoxCollider>();
            } else {
                Coll = transform.Find( colliderChild ).GetComponent<BoxCollider>();
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

            moveCountdownId = TimerManager.Create( this );
        }

        public virtual void FixedUpdate()
        {
            if (LastMinFallSpeed > Rb.velocity.y) {
                LastMinFallSpeed = Rb.velocity.y;
            }
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
            if (IsTouchingGround()) {
                Fallen( LastMinFallSpeed * -1 );
            }
        }

        public void OnDestroy()
        {
            // Remove this entity from contact area coz it not exists anymore
            if (ContactArea != null) {
                ContactArea.RemoveEntity( this );
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
                Move( moveSpeed.Current, transform.position.x - destination.x );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( flySpeed.Current, transform.position.x - destination.x );
        }

        public void Move(float moveSpeed, Vector2 destination)
        {
            Move( moveSpeed, transform.position.x - destination.x );
        }

        public void Move(Entity entity)
        {
            if (MovementStatus == MovementStatus.WALKING)
                Move( moveSpeed.Current, transform.position.x - entity.transform.position.x );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( flySpeed.Current, transform.position.x - entity.transform.position.x );
        }

        public void Move(float moveSpeed, Entity entity)
        {
            Move( moveSpeed, transform.position.x - entity.transform.position.x );
        }

        public void Move(Transform _transform)
        {
            if (MovementStatus == MovementStatus.WALKING)
                Move( moveSpeed.Current, transform.position.x - _transform.position.x );
            else if (MovementStatus == MovementStatus.JUMPING)
                Move( flySpeed.Current, transform.position.x - _transform.position.x );
        }

        public void Move(float moveSpeed, Transform _transform)
        {
            Move( moveSpeed, transform.position.x - _transform.position.x );
        }

        public void Move(float moveSpeed, float x)
        {
            if (CanMove && moveSpeed > 0) {
                LookDirection = x > 0 ? Vector2.right : Vector2.left;
                Rb.velocity = new Vector3( 0, Rb.velocity.y );
                transform.Translate( new Vector3( x * moveSpeed * Time.deltaTime, 0 ), Space.World );
                transform.rotation = Quaternion.LookRotation( new Vector3( 0, 0, LookDirection.x ), transform.up );
                OnMove( moveSpeed, x );
            }
        }

        public void Jump(float jumpPower)
        {
            Rb.velocity = new Vector2( Rb.velocity.x, jumpPower );
            LastMinFallSpeed = 0;
            MovementStatus = MovementStatus.JUMPING;
            IsInAir = true;
            OnJump( jumpPower );
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
            if (!( IsInviolability || blocked ) && attack.Damage > 0f) {
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
            Rb.freezeRotation = false;
            Rb.AddForce( new Vector3( UnityEngine.Random.Range( -14, 14 ), UnityEngine.Random.Range( -5, 25 ) ), ForceMode.VelocityChange );
            Rb.AddTorque( new Vector3( UnityEngine.Random.Range( 15, 35 ), UnityEngine.Random.Range( -25, 25 ), UnityEngine.Random.Range( -25, 25 ) ), ForceMode.VelocityChange );
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
                TimerManager.GetRemaing( moveCountdownId, out float seconds );
                if (seconds < pushDisableTime) {
                    // USTAWIA TIMER PO KTÓRYM JEDNOSTKA ODZYSKUJE MOŻLIWOŚĆ RUCHU
                    TimerManager.Reset( moveCountdownId, pushDisableTime );
                }
            }
            // NADAJE PRĘDKOŚĆ
            Rb.velocity = direction * pushPower;
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
            return Physics.CheckBox( new Vector3( Coll.bounds.center.x, Coll.bounds.min.y, Coll.bounds.center.z ),
                new Vector3( Coll.bounds.extents.x * 0.85f, Coll.bounds.extents.y * 0.1f, Coll.bounds.extents.z * 0.85f ),
                Coll.transform.rotation, groundLayers );
        }

        /// <summary>
        /// <br>Zamysł tej metody jest taki, że resetuje stan gracza ustawiajac jego hp i zasoby do 100%,</br>
        /// <br>usuwając wszystkie czasowe oraz pasywne buffy/debuffy (wliczajac w to wzmocnienia, stuny itd. itp)</br>
        /// </summary>
        public virtual void ResetUnit()
        {
            Rb.velocity = Vector3.zero;
            transform.position = startPosition;
            IsBlocking = false;
            CanMove = true;
            IsInviolability = false;
            LastMinFallSpeed = 0;
            IsDead = false;
            Rb.freezeRotation = true;
            Rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler( Vector3.zero );
            HitPoints.Current = HitPoints.Max;
        }

        public bool AddFollower(AIBehaviour follower)
        {
            return AiFollowers.Add( follower );
        }

        public bool RemoveFollower(AIBehaviour follower)
        {
            return AiFollowers.Remove( follower );
        }

        public virtual void OnContactAreaEnter(ContactArea contactArea)
        {
            foreach (AIBehaviour aiFollower in AiFollowers) {
                if (!aiFollower.FollowTarget( this )) {
                    aiFollower.StartPathRefind( pathRefindTimer );
                }
            }
        }
        public virtual void OnContactAreaExit(ContactArea contactArea)
        {

        }

        public virtual void OnCountdownEnd(long id, float overtime)
        {
            if (id == moveCountdownId) {
                CanMove = true;
            }
        }

        public void AddHitPointsObserver(IHitPointsObserver observer)
        {

        }

        public void RemoveHitPointsObserver(IHitPointsObserver observer)
        {

        }

        public void AddSourcePointsObserver(ISourcePointsObserver observer)
        {

        }

        public void RemoveSourcePointsObserver(ISourcePointsObserver observer)
        {

        }
        #endregion

        #region Abstract
        public abstract void OnMove(float moveSpeed, float x);
        public abstract void OnJump(float jumpPower);
        public abstract void OnFallen(float speedWhenFalling);
        public abstract void OnDamageTook(Attack attack);
        public abstract void OnRegenerate();
        public abstract void OnPushedOff(float pushPower, Vector3 direction, float disableTime);
        public abstract void OnBeingHealed(float healedHp);
        public abstract void OnDie();
        #endregion

        #region Setter And Getters
        public Vector3 LookRotation { get => new Vector3( LookDirection.x, 0 ); }
        public Attribute HitPoints { get => hitPoints; set => hitPoints = value; }
        public Attribute SourcePoints { get => sourcePoints; set => sourcePoints = value; }
        public Attribute MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public Attribute JumpPower { get => jumpPower; set => jumpPower = value; }
        public Attribute Damage { get => damage; set => damage = value; }
        public Attribute FlySpeed { get => flySpeed; set => flySpeed = value; }
        public float RegenHitPoints { get => regenHitPoints; set => regenHitPoints = value; }
        public float RegenSourcePoints { get => regenSourcePoints; set => regenSourcePoints = value; }
        public float DamageTakenMult { get => damageTakenMult; set => damageTakenMult = value; }
        public float FallDamageMult { get => fallDamageMult; set => fallDamageMult = value; }
        public bool IsDead { get => isDead; set => isDead = value; }
        public bool CanMove { get => canMove; set => canMove = value; }
        public bool IsInviolability { get => isInviolability; set => isInviolability = value; }
        public bool IsPaused { get => isPaused; set => isPaused = value; }
        public bool IsBlocking { get => isBlocking; set => isBlocking = value; }
        public bool IsPushImmune { get => isPushImmune; set => isPushImmune = value; }
        public bool IsFallDamagable { get => isFallDamagable; set => isFallDamagable = value; }
        public float PathRefindTimer { get => pathRefindTimer; set => pathRefindTimer = value; }
        public float MinDamagableFallSpeed { get => minDamagableFallSpeed; set => minDamagableFallSpeed = value; }
        public DialogueList[] DialogueLists { get => dialogueLists; set => dialogueLists = value; }
        public bool IsInAir { get => isInAir; set => isInAir = value; }
        public HashSet<AIBehaviour> AiFollowers { get => aiFollowers; set => aiFollowers = value; }
        public List<IHitPointsObserver> HitPointsObservers { get => hitPointsObservers; set => hitPointsObservers = value; }
        public List<ISourcePointsObserver> SourcePointsObservers { get => sourcePointsObservers; set => sourcePointsObservers = value; }
        public Rigidbody Rb { get => rb; set => rb = value; }
        public BoxCollider Coll { get => coll; set => coll = value; }
        public ContactArea ContactArea { get => contactArea; set => contactArea = value; }
        public Vector2 LookDirection { get => lookDirection; set => lookDirection = value; }
        public float LastMinFallSpeed { get => lastMinFallSpeed; set => lastMinFallSpeed = value; }
        public MovementStatus MovementStatus { get => movementStatus; set => movementStatus = value; }
        #endregion
    }
}