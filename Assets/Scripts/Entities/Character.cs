using ColdCry.Core;
using ColdCry.Utility;
using Inputs;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.Objects
{
    [RequireComponent( typeof( PInput ) )]
    public class Character : Entity,
        IStickListener,
        IButtonListener,
        ITriggerListener,
        IArrowsListener
    {
        #region Static Fields
        public static readonly float SIMPLE_ATTACK_TIME = 0.17f;
        public static readonly float FLY_ATTACK_TIME = 0.35f;
        public static readonly float BACKWARD_ATTACK_TIME = 0.53f;
        public static readonly float COMBO_BREAK_TIME = 0.15f;

        public static readonly float SIMPLE_ATTACK_MOVE = 5.75f;
        public static readonly float FLY_ATTACK_MOVE = 13.8f;

        /// <summary>
        /// Simple attack time disabling enemy movement
        /// </summary>
        public static readonly float SIMPLE_ATTACK_DISABLE_TIME = SIMPLE_ATTACK_TIME + 0.2f;
        /// <summary>
        /// Backward attack time disabling enemy movement
        /// </summary>
        public static readonly float BACKWARD_ATTACK_DISABLE_TIME = BACKWARD_ATTACK_TIME + 0.31f;
        /// <summary>
        /// Fly attack time disabling enemy movement
        /// </summary>
        public static readonly float FLY_ATTACK_DISABLE_TIME = FLY_ATTACK_TIME + 0.25f;
        #endregion

        #region Private Fields
        private bool doubleJumped = false;

        private GameObject mainSkill;
        private Animator animator;
        private List<Combo> combos = new List<Combo>();
        private LinkedList<ButtonCode> currentCombination = new LinkedList<ButtonCode>();
        private PInput input;
        private Weapon weapon;

        #region Countdowns IDs
        private long simpleAttackCountdown;
        private long flyAttackCountdown;
        private long backwardAttackCountdown;
        private long comboBreakCountdown;
        #endregion
        #endregion

        #region DEV TOOLS

        #endregion

        #region Unity API
        public override void Awake()
        {
            base.Awake();
            input = GetComponent<PInput>();
            mainSkill = GameObject.Find( "Mask" );
            animator = GetComponent<Animator>();
            weapon = FindObjectOfType<Weapon>();
        }

        public override void Start()
        {
            base.Start();
            simpleAttackCountdown = TimerManager.Start( SIMPLE_ATTACK_TIME, this );
            flyAttackCountdown = TimerManager.Start( FLY_ATTACK_TIME, this );
            backwardAttackCountdown = TimerManager.Start( BACKWARD_ATTACK_TIME, this );
            comboBreakCountdown = TimerManager.Start( COMBO_BREAK_TIME, this );

            /*Combo combo1 = new Combo( this, "Szakalaka",
                new ButtonCode[] { ButtonCode.Y, ButtonCode.A, ButtonCode.X, ButtonCode.B },
                () => { Debug.Log( "Szkalaka" ); } );

            Combo combo2 = new Combo( this, "WAKAMAKA FĄ",
                new ButtonCode[] { ButtonCode.Y, ButtonCode.A, ButtonCode.X, ButtonCode.B, ButtonCode.B },
                () => { Debug.Log( "WAKAMAKA FĄ" ); } );

            combos.Add( combo1 );
            combos.Add( combo2 );
            combos.Sort();*/
        }

        public override void FixedUpdate()
        {
            if (!IsDead) {
                base.FixedUpdate();
                animator.SetFloat( "speedY", Rb.velocity.y );
            }
        }
        #endregion

        #region Public Methods
        public override void OnMove(float moveSpeed, float x)
        {
            animator.SetBool( "isRunning", true );
        }

        public override void OnJump(float jumpPower)
        {
            animator.SetBool( "isInAir", true );
        }

        public override void OnDamageTook(Attack attack)
        {

        }

        public override void OnRegenerate()
        {

        }

        public override void OnFallen(float speedWhenFalling)
        {
            animator.SetBool( "isInAir", false );
        }

        public override void OnBeingHealed(float healedHp)
        {

        }

        public override void OnDie()
        {
            animator.SetFloat( "speedY", 0 );
            animator.SetBool( "isInAir", false );
            animator.SetTrigger( "isDying" );
        }

        public override void OnPushedOff(float pushPower, Vector3 direction, float disableTime)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawGizmos()
        {
            throw new System.NotImplementedException();
        }

        public override void ResetUnit()
        {
            base.ResetUnit();
            DialogueManager._Reset( true );
            animator.Play( "Player_Idle" );
        }

        /// <summary>
        /// Character do a simple left/right attack
        /// </summary>
        /// <param name="code">Button code to know if fast attack should be done</param>
        public void SimpleAtack()
        {
            // Jeżeli zakończył się timer odliczający czas od możliwego kolejnego ataku
            if (TimerManager.HasEnded( simpleAttackCountdown )) {
                // W innym wypadku jest on wykonany, ale jeżeli jesteśmy w aktualnie w locie
                // to atak jest wykonany w miejscu
                Vector3 attackDirection = new Vector3( SIMPLE_ATTACK_MOVE / 1.3f * LookDirection.x, Rb.velocity.y );
                if (IsTouchingGround())
                    Rb.velocity = attackDirection;
                // Przypisujemy ostatni klawisz ze zwykłego ataku oraz resetujemy timer
                TimerManager.Restart( simpleAttackCountdown );
                weapon.SetNextAttackInfo(
                    new Attack( Damage.Max,
                    (int) LookDirection.x,
                    new Vector2( (int)LookDirection.x, 0 ),
                    SIMPLE_ATTACK_MOVE,
                    SIMPLE_ATTACK_DISABLE_TIME ),
                    SIMPLE_ATTACK_TIME + 0.08F );
                animator.SetTrigger( "simpleAttack" );
                CanMove = false;
            }

        }

        /// <summary>
        /// Wykonanie ataku podrzucającego wroga oraz skaczącego postacią
        /// na niewielką wysokość
        /// </summary>
        public void FlyAttack()
        {
            if (TimerManager.HasEnded( flyAttackCountdown )) {
                TimerManager.Restart( flyAttackCountdown );
                Rb.velocity = new Vector3( Rb.velocity.x, FLY_ATTACK_MOVE );
                weapon.SetNextAttackInfo(
                    new Attack( Damage.Current,
                    new Vector2( 0, 1 ),
                    15,
                    FLY_ATTACK_DISABLE_TIME ),
                    FLY_ATTACK_TIME );
                animator.Play( "Player_JumpAttack", 0 );
            }
        }

        /// <summary>
        /// Atak wykonany w tył
        /// </summary>
        public void BackwardAttack()
        {
            if (TimerManager.HasEnded( backwardAttackCountdown )) {
                TimerManager.Restart( backwardAttackCountdown );
                LookDirection *= -1;
                Rb.velocity = new Vector3( SIMPLE_ATTACK_MOVE * 2 * LookDirection.x, Rb.velocity.y );
                CanMove = false;
                transform.rotation = Quaternion.LookRotation( new Vector3( 0, 0, LookDirection.x ), transform.up );
                weapon.SetNextAttackInfo(
                    new Attack( Damage.Current * 2,
                    (int) LookDirection.x,
                    new Vector3( LookDirection.x, 0 ),
                    SIMPLE_ATTACK_MOVE * 2,
                    BACKWARD_ATTACK_DISABLE_TIME ),
                    BACKWARD_ATTACK_TIME + 0.15F );
                animator.SetTrigger( "backwardAttack" );
            }
        }

        /// <summary>
        /// Sprawdza wszystkie możliwe combosy poczynając od combosów najbardziej
        /// skomplikowanych klawiszowo (najwięcej klawiszy), jeżeli których z nich
        /// jest poprawny to combos się wykona (tylko jeden combos może się wykonać)
        /// </summary>
        private void CheckCombos()
        {
            foreach (Combo combo in combos) {
                if (combo.IsValid( currentCombination )) {
                    combo.action();
                    return;
                }
            }
        }

        /// <summary>
        /// For timer when one from countdowns ends.
        /// </summary>
        /// <param name="id">Countdown id</param>
        public override void OnCountdownEnd(long id, float overtime)
        {
            base.OnCountdownEnd( id, overtime );
            if (moveCountdownId != id) {
                if (id == comboBreakCountdown && currentCombination.Count >= 3) {
                    CheckCombos();
                    currentCombination.Clear();
                } else if (simpleAttackCountdown == id ||
                            backwardAttackCountdown == id ||
                            flyAttackCountdown == id) {
                    CanMove = true;
                }
            }
        }
        #endregion

        #region Buttons
        public void OnButtonHeld(ButtonCode code)
        {
            if (!IsDead) {
                if (code == ButtonCode.RightBumper && CanMove) {
                    CanMove = false;
                    IsBlocking = true;
                    if (!animator.GetBool( "isBlocking" ))
                        animator.SetBool( "isBlocking", true );
                }
            }
        }

        public void OnButtonPressed(ButtonCode code)
        {
            if (!IsDead) {
                if (!IsPaused) {
                    TimerManager.Restart( comboBreakCountdown );
                    switch (code) {
                        case ButtonCode.A:
                            if (CanMove) {
                                if (IsTouchingGround()) {
                                    doubleJumped = false;
                                    Jump( JumpPower.Max );
                                } else if (!doubleJumped) {
                                    doubleJumped = true;
                                    Jump( JumpPower.Max * 0.7f );
                                }
                            }
                            currentCombination.AddLast( code );
                            break;
                        case ButtonCode.B:
                            if (CanMove) {
                                BackwardAttack();
                            }
                            currentCombination.AddLast( code );
                            break;
                        case ButtonCode.X:
                            if (CanMove) {
                                SimpleAtack();
                            }
                            currentCombination.AddLast( code );
                            break;
                        case ButtonCode.Y:
                            if (IsTouchingGround() && CanMove) {
                                FlyAttack();
                            }
                            currentCombination.AddLast( code );
                            break;
                        case ButtonCode.LeftBumper:
                            if (CanMove) {
                                // mainSkill.GetComponent<MainSkill>().ChangeWorld();
                            }
                            currentCombination.AddLast( code );
                            break;
                        case ButtonCode.RightBumper:
                            currentCombination.AddLast( code );
                            break;
                        case ButtonCode.Back:
                            break;
                        case ButtonCode.LeftStick:
                            TakeDamage( Attack.FullHP );
                            break;
                        case ButtonCode.RightStick:
                            break;
                    }
                    // Jeżeli kombinacje są za długie to są sprawdzane oraz czyszczone
                    if (currentCombination.Count >= 5) {
                        CheckCombos();
                        currentCombination.Clear();
                    }
                }
                if (code == ButtonCode.Back) {
                    if (DialogueManager.HasEndedDialogueList()) {
                        DialogueManager.StartDialogues( DialogueLists[0] );
                    } else {
                        DialogueManager.PushNextDialogue();
                    }
                }
            }
            if (code == ButtonCode.Start) {
                ResetUnit();
            }
            if (code == ButtonCode.RightStick) {
                GameManager.ResetAllEntities();
            }
        }

        public void OnButtonReleased(ButtonCode code)
        {
            if (!IsDead) {
                if (code == ButtonCode.RightBumper) {
                    IsBlocking = false;
                    CanMove = true;
                    animator.SetBool( "isBlocking", false );
                }
            }
        }
        #endregion

        #region Sticks
        public void OnStickChange(JoystickDoubleAxis stick)
        {
        }

        public void OnStickHold(JoystickDoubleAxis stick)
        {
            if (stick.Code == AxisCode.LeftStick && !( IsPaused || IsDead )) {
                Move( stick.X );
            }
        }

        public void OnStickDeadZone(JoystickDoubleAxis stick)
        {
            if (stick.Code == AxisCode.LeftStick && !( IsPaused || IsDead )) {
                animator.SetBool( "isRunning", false );
            }

        }
        #endregion

        #region Triggers
        public void OnTriggerChange(JoystickAxis trigger)
        {
            if (trigger.Code == AxisCode.LeftTrigger) {
                // DRUGI SKILL
            }
        }

        public void OnTriggerHold(JoystickAxis trigger)
        {
            if (trigger.Code == AxisCode.RightTrigger) {
                // BAZOWY ATAK
            }
        }

        public void OnTriggerDeadZone(JoystickAxis trigger)
        {
        }
        #endregion

        #region Arrows
        public void OnArrowsChange(JoystickDoubleAxis arrows)
        {
        }

        public void OnArrowsHold(JoystickDoubleAxis arrows)
        {
        }

        public void OnArrowsDeadZone(JoystickDoubleAxis arrows)
        {
        }
        #endregion

    }
}