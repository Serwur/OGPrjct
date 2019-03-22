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
    private static readonly float SIMPLE_ATTACK_TIME = 0.55f;
    private static readonly float FLY_ATTACK_TIME = 0.3f;
    private static readonly float BACKWARD_ATTACK_TIME = 0.47f;
    private static readonly float COMBO_BREAK_TIME = 0.15f;
    #endregion

    #region Private Fields
    private float simpleAttackSpeed = 5.75f;
    private float flyAttackSpeed = 11.5f;
    private int direction = 1;
    private float verticalVelocity = 0f;
    private bool doubleJumped = false;

    #region Countdowns IDs
    private long simpleAttackCountdown;
    private long flyAttackCountdown;
    private long backwardAttackCountdown;
    private long comboBreakCountdown;
    #endregion

    private ButtonCode lastLeftRightAtack;
    private List<Combo> combos = new List<Combo>();
    private LinkedList<ButtonCode> currentCombination = new LinkedList<ButtonCode>();
    private PInput input;
    private BoxCollider coll;

    #region Dev Fields
    // For dev test, use Start on Joystick or R on keyboard to reset position and velocity
    private Vector3 startPosition;
    #endregion
    #endregion

    #region Public Methods
    #region Unity API
    public override void Awake()
    {
        base.Awake();
        input = GetComponent<PInput>();
        coll = GetComponent<BoxCollider>();
    }

    public override void Start()
    {
        base.Start();
        simpleAttackCountdown = TimerManager.StartCountdown( SIMPLE_ATTACK_TIME, false, null );
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

    public void FixedUpdate()
    {
        if (rb.velocity.y < 0) {
            rb.velocity = new Vector3( rb.velocity.x, rb.velocity.y * 1.01f );
        }
    }
    #endregion

    /// <summary>
    /// Do zaimplementowania skrypt ruchu gracza.
    /// </summary>
    public void Move(float x)
    {
        if (canMove) {
            direction = x > 0 ? 1 : -1;
            //rb.velocity = new Vector3( x * moveSpeed.max, rb.velocity.y );
            rb.velocity = new Vector3( 0, rb.velocity.y );
            transform.Translate( new Vector3( x * moveSpeed.max * Time.deltaTime, 0 ) );
            /* Vector3 moveDirection = Vector3.zero;
             moveDirection.x = moveSpeed.max * Time.deltaTime * x;
             moveDirection.y = verticalVelocity;
             characterController.Move( moveDirection );*/
        }
    }

    /// <summary>
    /// Just a simple jump method
    /// </summary>
    public void Jump(float jumpPower)
    {
        rb.velocity = new Vector2( rb.velocity.x, jumpPower);
    }

    /// <summary>
    /// Character do a simple left/right attack
    /// </summary>
    /// <param name="code">Button code to know if fast attack should be done</param>
    public void SimpleAtack(ButtonCode code)
    {
        // Jeżeli atak jest wykonany z tego samego klawisza oraz nie zakończył się timer odliczający
        // czas od możliwego kolejnego ataku z tego samego klawisza, to elo
        if (lastLeftRightAtack == code && !TimerManager.HasEnded( simpleAttackCountdown )) {
            return;
        }

        // W innym wypadku jest on wykonany, ale jeżeli jesteśmy w aktualnie w locie
        // to atak jest wykonany w miejscu
        if (IsTouchingGround())
            rb.velocity = new Vector3( simpleAttackSpeed * direction, rb.velocity.y );

        // Przypisujemy ostatni klawisz ze zwykłego ataku oraz resetujemy timer
        lastLeftRightAtack = code;
        TimerManager.ResetCountdown( simpleAttackCountdown );
    }

    /// <summary>
    /// Wykonanie ataku podrzucającego wroga oraz skaczącego postacią
    /// na niewielką wysokość
    /// </summary>
    public void FlyAttack()
    {
        if (TimerManager.HasEnded( flyAttackCountdown )) {
            TimerManager.ResetCountdown( flyAttackCountdown );
            rb.velocity = new Vector3( rb.velocity.x, flyAttackSpeed );
        }
    }

    /// <summary>
    /// Atak wykonany w tył
    /// </summary>
    public void BackwardAttack()
    {
        if (TimerManager.HasEnded( backwardAttackCountdown )) {
            TimerManager.ResetCountdown( backwardAttackCountdown );
            direction *= -1;
            rb.velocity = new Vector3( simpleAttackSpeed * 2 * direction, rb.velocity.y );
            Debug.Log( "BACKWARD ATTACK" );
            canMove = false;
        }
    }

    #region Buttons
    public void OnButtonHeld(ButtonCode code)
    {
    }

    public void OnButtonPressed(ButtonCode code)
    {
        TimerManager.ResetCountdown( comboBreakCountdown );
        switch (code) {
            case ButtonCode.A:
                if (canMove) {
                    if (IsTouchingGround()) {
                        doubleJumped = false;
                        Jump(jumpPower.max);
                    } else if (!doubleJumped) {
                        doubleJumped = true;
                        Jump(jumpPower.max * 0.7f);
                    }
                }
                currentCombination.AddLast( code );
                break;
            case ButtonCode.B:
                SimpleAtack( code );
                currentCombination.AddLast( code );
                break;
            case ButtonCode.X:
                SimpleAtack( code );
                currentCombination.AddLast( code );
                break;
            case ButtonCode.Y:
                if (IsTouchingGround()) {
                    FlyAttack();
                }
                currentCombination.AddLast( code );
                break;
            case ButtonCode.LeftBumper:
                break;
            case ButtonCode.RightBumper:
                BackwardAttack();
                currentCombination.AddLast( code );
                break;
            case ButtonCode.Start:
                rb.velocity = Vector3.zero;
                transform.position = startPosition;
                break;
            case ButtonCode.Back:
                break;
            case ButtonCode.LeftStick:
                break;
            case ButtonCode.RightStick:
                break;
        }
        // Jeżeli kombinacje są za długie to są sprawdzane oraz czyszczone
        if (currentCombination.Count >= 8) {
            CheckCombos();
            currentCombination.Clear();
        }
    }

    public void OnButtonReleased(ButtonCode code)
    {
        switch (code) {
            case ButtonCode.A:
                break;
            case ButtonCode.B:
                break;
            case ButtonCode.X:
                break;
            case ButtonCode.Y:
                break;
            case ButtonCode.LeftBumper:
                break;
            case ButtonCode.RightBumper:
                break;
            case ButtonCode.Start:
                break;
            case ButtonCode.Back:
                break;
            case ButtonCode.LeftStick:
                break;
            case ButtonCode.RightStick:
                break;
        }
    }
    #endregion

    #region Sticks
    public void OnStickChange(JoystickDoubleAxis stick)
    {
    }

    public void OnStickHold(JoystickDoubleAxis stick)
    {
        switch (stick.Code) {
            case AxisCode.LeftStick:
                Move( stick.X );
                break;
            case AxisCode.RightStick:
                break;
        }
    }

    public void OnStickDeadZone(JoystickDoubleAxis stick)
    {

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

    public override void ResetUnit()
    {
        throw new System.NotImplementedException();
    }

    public override void Die()
    {
        throw new System.NotImplementedException();
    }

    public void OnCountdownEnd(long id)
    {
        if (id == backwardAttackCountdown) {
            canMove = true;
        } else if (id == comboBreakCountdown) {
            if (currentCombination.Count >= 3) {
                CheckCombos();
            }
            currentCombination.Clear();
        }
    }
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