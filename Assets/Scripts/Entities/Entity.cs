using DoubleMMPrjc.AI;
using DoubleMMPrjc.Timer;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    [RequireComponent( typeof( Rigidbody ) )]
    public abstract class Entity : MonoBehaviour, IOnCountdownEnd
    {
        public static readonly float MIN_DAMAGEABLE_FALL_SPEED = 30f;

        [Header( "Attributes" )]
        public Attribute hitPoints;
        public Attribute sourcePoints;
        public Attribute armor;
        public Attribute moveSpeed;
        public Attribute jumpPower;
        public Attribute damage;
        public Attribute jumpSpeed;

        [Header( "Regeneration" )]
        public float regenHitPoints = 0f;
        public float regenSourcePoints = 0f;

        [Header( "Multiplayers" )]
        [Range( 0.01f, 10f )] public float damageTakenMult = 1f;
        [Range( 0.01f, 10f )] public float fallDamageMult = 1f;

        [Header( "Utility" )]
        [SerializeField] protected bool isDead = false;
        [SerializeField] protected bool canMove = true;
        [SerializeField] protected bool isInviolability = false;
        [SerializeField] protected bool isPaused = false;
        [SerializeField] protected bool isBlocking = false;
        [SerializeField] protected bool isPushImmune = false;
        [SerializeField] protected bool isFallDamagable = true;

        [Header( "Dialogs" )]
        public DialogueList[] dialogueLists;

        [Header( "Others" )]
        public LayerMask groundLayers;
        [Range( 0.05f, 1.5f )] public float pathRefindTimer = 0.6f;

        #region Dev Fields
        [Header( "Dev Tools" )]
        public bool anotherCollider = false;
        public string colliderChild = "";
        // For dev test, use Start on Joystick or R on keyboard to reset position and velocity
        public Vector3 startPosition;
        #endregion

        #region Protected Fields
        [SerializeField] private HashSet<NPC> followers = new HashSet<NPC>();

        protected Rigidbody rb;
        protected BoxCollider coll;
        [SerializeField] protected ContactArea contactArea;
        // -1 = left, 1 = right
        protected Vector2 lookDirection = new Vector2(1,0);
        protected float lastMinFallSpeed = float.MaxValue;
        protected long moveCountdownId;
        #endregion

        #region Unity API
        public virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            // DEV TOOL TO DELETE IN FUTURE
            if (!anotherCollider) {
                coll = GetComponent<BoxCollider>();
            } else {
                coll = transform.Find( colliderChild ).GetComponent<BoxCollider>();
            }
            startPosition = transform.position;
        }

        public virtual void Start()
        {
            GameManager.AddEntity( this );
            UpdateAttributes();
            hitPoints.current = hitPoints.max;
            sourcePoints.current = sourcePoints.max;
            armor.current = armor.max;
            moveSpeed.current = moveSpeed.max;
            jumpPower.current = jumpPower.max;
            damage.current = damage.max;

            moveCountdownId = TimerManager.Create( this );
        }

        public virtual void FixedUpdate()
        {
            if (lastMinFallSpeed > rb.velocity.y) {
                lastMinFallSpeed = rb.velocity.y;
            }
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
            if (IsTouchingGround()) {
                OnFallen( lastMinFallSpeed * -1 );
                lastMinFallSpeed = float.MaxValue;
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
        /// Zadaje obrażenia jednostce.
        /// </summary>
        /// <param name="attack">Informacje o wykonywanym ataku</param>
        /// <returns>Zwraca TRUE, jeżeli jednostka otrzymała jakiekolwiek obrażenia w przeciwnym wypadku zwraca FALSE</returns>
        public virtual bool TakeDamage(Attack attack)
        {
            // SPRAWDZA CZY JEDNOSTKA NIE ZABLOKOWAŁA ATAKU
            bool blocked = ShouldBlockAttack( attack );
            // JEŻELI JEDNOSTKA NIE JEST ODPORNA NA ODEPCHNIĘCIA I MOC
            // ODEPCHNIĘCIA JEST WIĘKSZA OD 0 TO JĄ ODPYCHA
            if (!isPushImmune && attack.PushPower > 0f) {
                Throw( attack, blocked );
            }
            // JEŻELI JEDNOSTKA JEST NIETYKALNA, ZABLOKOWAŁA ATAK
            // ORAZ ZADAWANE OBRAŻENIA SĄ WIĘKSZE NIŻ 0 TO TRACI PUNKTY ŻYCIA
            if (!( isInviolability || blocked ) && attack.Damage > 0f) {
                if (attack.PercenteDamage) {
                    hitPoints.current -= hitPoints.max * attack.Damage;
                } else {
                    hitPoints.current -= attack.Damage;
                }
                if (hitPoints.current <= 0f)
                    Die();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Odrzuca jednostkę nadając jej pewną prędkość. Można przy tym wyłączyć możliwość ruchu
        /// na pewną ilość sekund.
        /// </summary>
        /// <param name="pushPower">Siła odrzutu</param>
        /// <param name="direction">Kierunek odrzutu</param>
        /// <param name="pushDisableTime">Czas wstrzymania możliwości ruchu, jeżeli 0 lub mniej to ruch
        /// nie zostaje wyłączony</param>
        public virtual void Throw(float pushPower, Vector3 direction, float pushDisableTime, bool blocked = false)
        {
            // JEŻELI PUSH DISABLE TIME > 0 I JEDNOSTKA NIE BLOKUJE TO UNIEAKTYWNIA RUCH
            if (pushDisableTime > 0 && !blocked) {
                canMove = false;
                TimerManager.GetRemaing( moveCountdownId, out float seconds );
                if (seconds < pushDisableTime) {
                    // USTAWIA TIMER PO KTÓRYM JEDNOSTKA ODZYSKUJE MOŻLIWOŚĆ RUCHU
                    TimerManager.Reset( moveCountdownId, pushDisableTime );
                }
            }
            // NADAJE PRĘDKOŚĆ
            rb.velocity = direction * pushPower;
        }

        public virtual void Throw(Attack attack, bool blocked = false)
        {
            Throw( attack.PushPower, attack.PushDirection, attack.PushDisableTime, blocked );
        }

        /// <summary>
        /// <br>Metoda wywoływana co sekundę w celu regeneracji zycia oraz zasobów.</br>
        /// <br>Powinna być wywołana w jednej klasie, która będzie mieć dostep do wszystkich jednostek np. GameMaster.</br>
        /// </summary>
        public virtual void Regenerate()
        {
            float nextHitPoints = hitPoints.current + regenHitPoints;
            if (nextHitPoints > hitPoints.max)
                hitPoints.current = hitPoints.max;
            else
                hitPoints.current = nextHitPoints;

            float nextSourcePoints = sourcePoints.current + regenSourcePoints;
            if (nextSourcePoints > sourcePoints.max)
                sourcePoints.current = hitPoints.max;
            else
                sourcePoints.current = nextSourcePoints;
        }

        /// <summary>
        /// Metoda lecząca jednostkę o podane punkty zdrowia. Nadpisać metodę w klasach pochodnych, jeżeli istnieje taka potrzeba.
        /// </summary>
        /// <param name="heal">Ilość punktów zdrowia do uleczenia</param>
        /// <returns>TRUE jeżeli zwiększono ilość punktów zdrowia, w przeciwnym wypadku FALSE</returns>
        public virtual bool Heal(float heal)
        {
            float nextHitPoints = hitPoints.current + heal;
            if (nextHitPoints > hitPoints.max)
                hitPoints.current = hitPoints.max;
            else
                hitPoints.current = nextHitPoints;
            return true;
        }

        /// <summary>
        /// <br>Aktualizuje statystyki jednostki takie jak maksymalna ilość zdrowia, maksymalna ilość zasobów, atak, pancerz itp. itd.</br>
        /// <br>Metoda powinna być wywoływana w momencie zmiany którejś ze statystyk zwiększających lub zmniejsząjących obrażenia, życie maksymalne itp..</br>
        /// </summary>
        public virtual void UpdateAttributes()
        {
            hitPoints.UpdateAttribute();
            sourcePoints.UpdateAttribute();
            armor.UpdateAttribute();
            moveSpeed.UpdateAttribute();
            jumpPower.UpdateAttribute();
            damage.UpdateAttribute();
            jumpSpeed.UpdateAttribute();
        }

        /// <summary>
        /// Checks if attack should be blocked
        /// </summary>
        /// <returns><code>TRUE</code> if attack should be blocked, otherwise <code>FALSE</code></returns>
        public virtual bool ShouldBlockAttack(Attack attack)
        {
            if (!isBlocking && attack.AttackDirection == 0)
                return false;
            return attack.AttackDirection != lookDirection.x;
        }

        public virtual void OnFallen(float speedWhenFalling)
        {
            if (isFallDamagable && speedWhenFalling > MIN_DAMAGEABLE_FALL_SPEED) {
                TakeDamage(
                    new Attack(
                        ( speedWhenFalling - MIN_DAMAGEABLE_FALL_SPEED ) / MIN_DAMAGEABLE_FALL_SPEED,
                        true ) );
            }
        }

        /// <summary>
        /// Checks if entity is touching ground
        /// </summary>
        /// <returns>TRUE if character is touching ground, otherwise FALSE</returns>
        public virtual bool IsTouchingGround()
        {
            return Physics.CheckBox( new Vector3( coll.bounds.center.x, coll.bounds.min.y, coll.bounds.center.z ),
                new Vector3( coll.bounds.extents.x * 0.85f, coll.bounds.extents.y * 0.1f, coll.bounds.extents.z * 0.85f ),
                coll.transform.rotation, groundLayers );
        }

        /// <summary>
        /// <br>Zamysł tej metody jest taki, że resetuje stan gracza ustawiajac jego hp i zasoby do 100%,</br>
        /// <br>usuwając wszystkie czasowe oraz pasywne buffy/debuffy (wliczajac w to wzmocnienia, stuny itd. itp)</br>
        /// </summary>
        public virtual void ResetUnit()
        {
            rb.velocity = Vector3.zero;
            transform.position = startPosition;
            isBlocking = false;
            canMove = true;
            isInviolability = false;
            lastMinFallSpeed = 0;
            isDead = false;
            rb.freezeRotation = true;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler( Vector3.zero );
            hitPoints.current = hitPoints.max;
        }

        /// <summary>
        /// <br>Zabija jednostkę wykonując przy tym zaimplementowane działania np. wykonywanie animacji śmierci, dodanie</br>
        /// <br>punktów graczom itp. itd.</br>
        /// </summary>
        public virtual void Die()
        {
            rb.freezeRotation = false;
            rb.AddForce( new Vector3( Random.Range( -14, 14 ), Random.Range( -5, 25 ) ), ForceMode.VelocityChange );
            rb.AddTorque( new Vector3( Random.Range( 15, 35 ), Random.Range( -25, 25 ), Random.Range( -25, 25 ) ), ForceMode.VelocityChange );
            isDead = true;
        }

        public virtual void Jump(float jumpPower)
        {
            rb.velocity = new Vector2( rb.velocity.x, jumpPower );
            lastMinFallSpeed = 0;
        }

        public virtual bool Move(float moveSpeed)
        {
            if (moveSpeed <= 0 && !canMove)
                return false;
            transform.Translate( lookDirection * moveSpeed * Time.fixedDeltaTime, Space.World );
            return true;
        }

        public void TurnTo(float x)
        {
            if (x >= 1)
                TurnRight();
            else
                TurnLeft();
        }

        public void TurnLeft()
        {
            lookDirection.x = -1;
            ChangeDirection();
        }

        public void TurnRight()
        {
            lookDirection.x = 1;
            ChangeDirection();
        }

        public void TurnOpposite()
        {
            lookDirection.x *= -1;
            ChangeDirection();
        }

        public virtual void OnCountdownEnd(long id)
        {
            if (id == moveCountdownId) {
                canMove = true;
            }
        }

        public bool AddFollower(NPC follower)
        {
            return followers.Add( follower );
        }

        public bool RemoveFollower(NPC follower)
        {
            return followers.Remove( follower );
        }

        public virtual void OnContactAreaEnter(ContactArea contactArea)
        {
            /* foreach (NPC npc in FollowersList) {
                 if (!npc.FollowTarget( this )) {
                     npc.StartPathRefind( pathRefindTimer );
                 }

                 //bool b = npc.FollowTarget( this );
                 /* if (npc.ContactArea != null && !npc.FollowTarget( this )) {
                      npc.SetReachState();
                      //npc.SetWatchState();
                      Debug.Log("refind by enter");

                  }
                  Debug.Log("dont refind again");
             }*/
        }

        private void ChangeDirection()
        {
            transform.rotation = Quaternion.LookRotation( new Vector3( 0, 0, lookDirection.x ), transform.up );
        }

        public abstract void OnContactAreaExit(ContactArea contactArea);
        #endregion

        #region Setter And Getters
        public bool IsPaused { get => isPaused; set => isPaused = value; }
        public Vector2 LookRotation { get => lookDirection; }
        public ContactArea ContactArea { get => contactArea; set => contactArea = value; }
        public Entity[] FollowersList { get => Utility.Collections.ToArray( followers ); }
        #endregion
    }
}