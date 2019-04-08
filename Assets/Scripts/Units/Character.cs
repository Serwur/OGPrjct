using Inputs;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( PInput ) )]
public class Character : Entity, IStickListener, IButtonListener,
    ITriggerListener, IArrowsListener, TimerManager.IOnCountdownEnd
{
    #region Public Fields
    [Header( "Others" )]
    public LayerMask groundLayers;
    #endregion

    #region Static Fields
    private static readonly float SIMPLE_ATTACK_TIME = 0.17f;
    private static readonly float FLY_ATTACK_TIME = 0.3f;
    private static readonly float BACKWARD_ATTACK_TIME = 0.47f;
    private static readonly float COMBO_BREAK_TIME = 0.15f;
    private static readonly float SIMPLE_ATTACK_MOVE = 5.75f;
    private static readonly float FLY_ATTACL_MOVE = 11.5f;
    #endregion

    #region Private Fields

    private bool doubleJumped = false;
    private bool shouldCheckIfIsInAir = false;

    private GameObject mainSkill;
    private Animator animator;
    private List<Combo> combos = new List<Combo>();
    private LinkedList<ButtonCode> currentCombination = new LinkedList<ButtonCode>();
    private PInput input;
    private BoxCollider coll;
    private Weapon weapon;

    #region Countdowns IDs
    private long simpleAttackCountdown;
    private long flyAttackCountdown;
    private long backwardAttackCountdown;
    private long comboBreakCountdown;
    #endregion

    #region Dev Fields
    [Header( "Dev Tools" )]
    public bool anotherCollider = true;
    public string colliderChild = "";
    // For dev test, use Start on Joystick or R on keyboard to reset position and velocity
    private Vector3 startPosition;
    #endregion
    #endregion

    #region Unity API
    public override void Awake()
    {
        base.Awake();
        input = GetComponent<PInput>();
        // DEV TOOL TO DELETE IN FUTURE
        if (!anotherCollider) {
            coll = GetComponent<BoxCollider>();
        } else {
            coll = transform.Find( colliderChild ).GetComponent<BoxCollider>();
        }
        mainSkill = GameObject.Find("Mask");
        animator = GetComponent<Animator>();
        weapon = FindObjectOfType<Weapon>();
    }

    public override void Start()
    {
        base.Start();
        simpleAttackCountdown = TimerManager.StartCountdown( SIMPLE_ATTACK_TIME, false, this );
        flyAttackCountdown = TimerManager.StartCountdown( FLY_ATTACK_TIME, false, null );
        backwardAttackCountdown = TimerManager.StartCountdown( BACKWARD_ATTACK_TIME, false, this );
        comboBreakCountdown = TimerManager.StartCountdown( COMBO_BREAK_TIME, false, this );

        Combo combo1 = new Combo( this, "Szakalaka",
            new ButtonCode[] { ButtonCode.Y, ButtonCode.A, ButtonCode.X, ButtonCode.B },
            () => { Debug.Log( "Szkalaka" ); } );

        Combo combo2 = new Combo( this, "WAKAMAKA FĄ",
            new ButtonCode[] { ButtonCode.Y, ButtonCode.A, ButtonCode.X, ButtonCode.B, ButtonCode.B },
            () => { Debug.Log( "WAKAMAKA FĄ" ); } );

        combos.Add( combo1 );
        combos.Add( combo2 );
        combos.Sort();
        startPosition = transform.position;
    }

    public void Update()
    {
        if (animator != null && shouldCheckIfIsInAir) {
            shouldCheckIfIsInAir = false;
            if (Mathf.Clamp( rb.velocity.y, -0.1f, 0.1f ) == rb.velocity.y)
                animator.SetBool( "isInAir", false );
        }
    }

    public void FixedUpdate()
    {
        if (rb.velocity.y < 0) {
            rb.velocity = new Vector3( rb.velocity.x, rb.velocity.y * 1.01f );
        }
        if (animator != null) {
            animator.SetFloat( "speedY", rb.velocity.y );
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (IsTouchingGround())
            shouldCheckIfIsInAir = true;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Do zaimplementowania skrypt ruchu gracza.
    /// </summary>
    public void Move(float x)
    {
        if (canMove) {
            lookDirection = x > 0 ? 1 : -1;
            rb.velocity = new Vector3( 0, rb.velocity.y );
            if (animator != null)
                animator.SetBool( "isRunning", true );
            transform.Translate( new Vector3( x * moveSpeed.max * Time.deltaTime, 0 ), Space.World );
            transform.rotation = Quaternion.LookRotation( new Vector3( 0, 0, lookDirection ), transform.up );
        }
    }

    /// <summary>
    /// Just a simple jump method
    /// </summary>
    public void Jump(float jumpPower)
    {
        rb.velocity = new Vector2( rb.velocity.x, jumpPower );
        if (animator != null)
            animator.SetBool( "isInAir", true );
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
            Vector3 attackDirection = new Vector3( SIMPLE_ATTACK_MOVE * lookDirection, rb.velocity.y );
            if (IsTouchingGround())
                rb.velocity = attackDirection;
            // Przypisujemy ostatni klawisz ze zwykłego ataku oraz resetujemy timer
            TimerManager.ResetCountdown( simpleAttackCountdown );
            weapon.SetNextAttackInfo( new Attack( 1, new Vector3( lookDirection, 0 ), SIMPLE_ATTACK_MOVE, 0.17f ) );
            animator.Play( "Player_SimpleAttack", 0 );
            canMove = false;
        }
    }

    /// <summary>
    /// Wykonanie ataku podrzucającego wroga oraz skaczącego postacią
    /// na niewielką wysokość
    /// </summary>
    public void FlyAttack()
    {
        if (TimerManager.HasEnded( flyAttackCountdown )) {
            TimerManager.ResetCountdown( flyAttackCountdown );
            rb.velocity = new Vector3( rb.velocity.x, FLY_ATTACL_MOVE );
            animator.Play( "Player_JumpAttack", 0 );
        }
    }

    /// <summary>
    /// Atak wykonany w tył
    /// </summary>
    public void BackwardAttack()
    {
        if (TimerManager.HasEnded( backwardAttackCountdown )) {
            TimerManager.ResetCountdown( backwardAttackCountdown );
            lookDirection *= -1;
            rb.velocity = new Vector3( SIMPLE_ATTACK_MOVE * 2 * lookDirection, rb.velocity.y );
            canMove = false;
            if (animator != null) {
                animator.SetTrigger( "backwardAttack" );
            }
        }
    }

    public override bool ShouldBlockAttack(Attack attack)
    {
        return base.ShouldBlockAttack( attack );
    }

    public override void ResetUnit()
    {
        rb.velocity = Vector3.zero;
        transform.position = startPosition;
        isBlocking = false;
        canMove = true;
        isInviolability = false;
        DialogueManager._Reset();
    }

    public override void Die()
    {

    }

    /// <summary>
    /// For timer when one from countdowns ends.
    /// </summary>
    /// <param name="id">Countdown id</param>
    public void OnCountdownEnd(long id)
    {
        if (id == backwardAttackCountdown) {
            canMove = true;
            transform.rotation = Quaternion.LookRotation( new Vector3( 0, 0, lookDirection ), transform.up );
        } else if (id == comboBreakCountdown) {
            if (currentCombination.Count >= 3) {
                CheckCombos();
            }
            currentCombination.Clear();
        } else if (id == simpleAttackCountdown) {
            canMove = true;
            weapon.SetNextAttackInfo( null );
        }
    }

    #region Buttons
    public void OnButtonHeld(ButtonCode code)
    {
        if (code == ButtonCode.RightBumper && canMove) {
            canMove = false;
            isBlocking = true;
            if (!animator.GetBool( "isBlocking" ))
                animator.SetBool( "isBlocking", true );
        }
    }

    public void OnButtonPressed(ButtonCode code)
    {
        if (!isPaused) {
            TimerManager.ResetCountdown( comboBreakCountdown );
            switch (code) {
                case ButtonCode.A:
                    if (canMove) {
                        if (IsTouchingGround()) {
                            doubleJumped = false;
                            Jump( jumpPower.max );
                        } else if (!doubleJumped) {
                            doubleJumped = true;
                            Jump( jumpPower.max * 0.7f );
                        }
                    }
                    currentCombination.AddLast( code );
                    break;
                case ButtonCode.B:
                    if (canMove) {
                        BackwardAttack();
                    }
                    currentCombination.AddLast( code );
                    break;
                case ButtonCode.X:
                    if (canMove) {
                        SimpleAtack();
                    }
                    currentCombination.AddLast( code );
                    break;
                case ButtonCode.Y:
                    if (IsTouchingGround() && canMove) {
                        FlyAttack();
                    }
                    currentCombination.AddLast( code );
                    break;
                case ButtonCode.LeftBumper:
                    Debug.Log( "0" );
                    if (canMove) {
                        Debug.Log( "1" );
                        mainSkill.GetComponent<MainSkill>().ChangeWorld();
                    }
                    currentCombination.AddLast( code );
                    break;
                case ButtonCode.RightBumper:
                    currentCombination.AddLast( code );
                    break;
                case ButtonCode.Start:
                    ResetUnit();
                    break;
                case ButtonCode.Back:
                    break;
                case ButtonCode.LeftStick:
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
                DialogueManager.StartDialogues( dialogueLists[0] );
            } else {
                DialogueManager.PushNextDialogue();
            }
        }
    }

    public void OnButtonReleased(ButtonCode code)
    {
        if (code == ButtonCode.RightBumper) {
            isBlocking = false;
            canMove = true;
            animator.SetBool( "isBlocking", false );
        }
    }
    #endregion

    #region Sticks
    public void OnStickChange(JoystickDoubleAxis stick)
    {
    }

    public void OnStickHold(JoystickDoubleAxis stick)
    {
        if (!IsPaused && stick.Code == AxisCode.LeftStick) {
            Move( stick.X );
        }
    }

    public void OnStickDeadZone(JoystickDoubleAxis stick)
    {
        if (stick.Code == AxisCode.LeftStick && animator != null) {
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
    #endregion

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
    /// Checks if character is touching ground
    /// </summary>
    /// <returns>TRUE if character is touching ground, otherwise FALSE</returns>
    private bool IsTouchingGround()
    {
        return Physics.CheckBox( new Vector3( coll.bounds.center.x, coll.bounds.min.y, coll.bounds.center.z ),
            new Vector3( coll.bounds.extents.x * 0.85f, coll.bounds.extents.y * 0.1f, coll.bounds.extents.z * 0.85f ),
            coll.transform.rotation, groundLayers );
    }
}