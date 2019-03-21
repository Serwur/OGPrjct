using UnityEngine;

namespace Inputs
{
    /// <summary>
    /// REMEMBER - GameObject that you want to use this class needs to be component of this object.
    /// </summary>
    public class PInput : MonoBehaviour
    {
        [Header( "Player Setup" )]
        [Range( 1, 4 )]
        [SerializeField]
        private int playerNumber = 1;
        [SerializeField]
        private bool usesJoystick = true;
        [SerializeField]
        private KeyboardSettings alternativeKeyboard = null;
        [SerializeField]
        private bool inputEnabled = true;

        #region DeadZones
        [Header( "Joystick Dead Zone" )]
        [Range( 0f, 1f )]
        [SerializeField]
        private float LSx = 0.15f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float LSy = 0.15f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float RSx = 0.15f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float RSy = 0.15f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float ArrowsX = 0.10f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float ArrowsY = 0.10f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float LT = 0.10f;
        [Range( 0f, 1f )]
        [SerializeField]
        private float RT = 0.10f;
        #endregion

        #region Listeners
        private IButtonListener buttonInput;
        private IStickListener stickInput;
        private ITriggerListener triggerInput;
        private IArrowsListener arrowsInput;
        #endregion

        #region Buttons
        private JoystickButton buttonA, buttonB, buttonX, buttonY;
        private JoystickButton buttonStart, buttonBack;
        private JoystickButton buttonLeftBumper, buttonRightBumper;
        private JoystickButton buttonLeftStick, buttonRightStick;
        private JoystickAxis leftTrigger, rightTrigger;
        private JoystickDoubleAxis leftStick, rightStick, arrows;
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if button is held.
        /// </summary>
        /// <param name="code">Button to check</param>
        /// <returns>True if button is held, otherwise false.</returns>
        public bool IsHeld(ButtonCode code)
        {
            switch (code) {
                case ButtonCode.A:
                    return buttonA.IsHeld;
                case ButtonCode.B:
                    return buttonB.IsHeld;
                case ButtonCode.X:
                    return buttonX.IsHeld;
                case ButtonCode.Y:
                    return buttonY.IsHeld;
                case ButtonCode.LeftBumper:
                    return buttonLeftBumper.IsHeld;
                case ButtonCode.RightBumper:
                    return buttonRightBumper.IsHeld;
                case ButtonCode.Start:
                    return buttonStart.IsHeld;
                case ButtonCode.Back:
                    return buttonBack.IsHeld;
                case ButtonCode.LeftStick:
                    return buttonLeftStick.IsHeld;
                case ButtonCode.RightStick:
                    return buttonRightStick.IsHeld;
            }
            return false;
        }

        /// <summary>
        /// Checks if axis is in dead zone
        /// </summary>
        /// <param name="code">Code of axis</param>
        /// <returns>TRUE if is in dead zone, otherwise returns FALSE</returns>
        public bool IsInDeadZone(AxisCode code)
        {
            switch (code) {
                case AxisCode.LeftStick:
                    return IsInDead( leftStick.X, leftStick.DeadZoneX ) && IsInDead( leftStick.Y, leftStick.DeadZoneY );
                case AxisCode.RightStick:
                    return IsInDead( rightStick.X, rightStick.DeadZoneX ) && IsInDead( rightStick.Y, rightStick.DeadZoneY );
                case AxisCode.Arrows:
                    return IsInDead( arrows.X, arrows.DeadZoneX ) && IsInDead( arrows.Y, arrows.DeadZoneY );
                case AxisCode.LeftTrigger:
                    return IsBelowDead( leftTrigger.X, leftTrigger.DeadZoneX );
                case AxisCode.RightTrigger:
                    return IsBelowDead( rightTrigger.X, rightTrigger.DeadZoneX );
            }
            return false;
        }
        #endregion

        #region Private Methods
        private void Awake()
        {
            buttonInput = GetComponent<IButtonListener>();
            stickInput = GetComponent<IStickListener>();
            arrowsInput = GetComponent<IArrowsListener>();
            triggerInput = GetComponent<ITriggerListener>();
        }

        private void Start()
        {
            SetupButtons();
        }

        /// <summary>
        /// Setup buttons input names
        /// </summary>
        private void SetupButtons()
        {
            SetupSticks( "Left-X-Stick-" + playerNumber, "Left-Y-Stick-" + playerNumber,
               "Right-X-Stick-" + playerNumber, "Right-Y-Stick-" + playerNumber );
            SetupTriggers( "Left-Trigger-" + playerNumber, "Right-Trigger-" + playerNumber );
            SetupButtons( "Button-A-" + playerNumber, "Button-B-" + playerNumber,
                        "Button-X-" + playerNumber, "Button-Y-" + playerNumber );
            SetupArrows( "Arrows-Horizontal-" + playerNumber, "Arrows-Vertical-" + playerNumber );
            SetupStickButtons( "Left-Stick-Button-" + playerNumber, "Right-Stick-Button-" + playerNumber );
            SetupMenusButtons( "Button-Start-" + playerNumber, "Button-Back-" + playerNumber );
            SetupBumpers( "Left-Bumper-" + playerNumber, "Right-Bumper-" + playerNumber );
        }

        private void SetupSticks(string left_x_stick, string left_y_stick,
            string right_x_stick, string right_y_stick)
        {
            leftStick = new JoystickDoubleAxis( this, left_x_stick, left_y_stick, AxisCode.LeftStick, LSx, LSy );
            rightStick = new JoystickDoubleAxis( this, right_x_stick, right_y_stick, AxisCode.RightStick, RSx, RSy );
        }

        private void SetupTriggers(string left_trigger, string right_trigger)
        {
            leftTrigger = new JoystickAxis( this, left_trigger, AxisCode.LeftTrigger, LT );
            rightTrigger = new JoystickAxis( this, right_trigger, AxisCode.RightTrigger, RT );
        }

        private void SetupButtons(string button_a, string button_b, string button_x, string button_y)
        {
            buttonA = new JoystickButton( this, button_a, ButtonCode.A );
            buttonB = new JoystickButton( this, button_b, ButtonCode.B );
            buttonX = new JoystickButton( this, button_x, ButtonCode.X );
            buttonY = new JoystickButton( this, button_y, ButtonCode.Y );
        }

        private void SetupArrows(string horizontal_arrows, string vertical_arrows)
        {
            arrows = new JoystickDoubleAxis( this, horizontal_arrows, vertical_arrows, AxisCode.Arrows, ArrowsX, ArrowsY );
        }

        private void SetupStickButtons(string button_left_stick, string button_right_stick)
        {
            buttonLeftStick = new JoystickButton( this, button_left_stick, ButtonCode.LeftStick );
            buttonRightStick = new JoystickButton( this, button_right_stick, ButtonCode.RightStick );
        }

        private void SetupMenusButtons(string button_start, string button_back)
        {
            buttonStart = new JoystickButton( this, button_start, ButtonCode.Start );
            buttonBack = new JoystickButton( this, button_back, ButtonCode.Back );
        }

        private void SetupBumpers(string left_bumper, string right_bumper)
        {
            buttonLeftBumper = new JoystickButton( this, left_bumper, ButtonCode.LeftBumper );
            buttonRightBumper = new JoystickButton( this, right_bumper, ButtonCode.RightBumper );
        }

        private void FixedUpdate()
        {
            /*  string [] joys =  Input.GetJoystickNames();
              for ( int i = 0;  i < joys.Length; i++) {
                  if ( joys[i] == null || joys[i].Length == 0) {
                      Debug.Log("Its shit");
                  } else {
                      Debug.Log(joys[i]);
                  }
              }*/
            if (inputEnabled) {
                if (stickInput != null) {
                    StickAxisUpdate( leftStick );
                    StickAxisUpdate( rightStick );
                }
                if (triggerInput != null) {
                    TriggerAxisUpdate( leftTrigger );
                    TriggerAxisUpdate( rightTrigger );
                }
                if (arrowsInput != null) {
                    ArrowsAxisUpdate( arrows );
                }
                if (buttonInput != null) {
                    ButtonUpdate( buttonA );
                    ButtonUpdate( buttonB );
                    ButtonUpdate( buttonX );
                    ButtonUpdate( buttonY );
                    ButtonUpdate( buttonStart );
                    ButtonUpdate( buttonBack );
                    ButtonUpdate( buttonLeftBumper );
                    ButtonUpdate( buttonRightBumper );
                    ButtonUpdate( buttonLeftStick );
                    ButtonUpdate( buttonRightStick );
                }
            }
        }

        private void ButtonUpdate(JoystickButton button)
        {
            if (button.GetButton()) {
                if (!button.IsHeld) {
                    buttonInput.OnButtonPressed( button.Code );
                    button.IsHeld = true;
                }
                buttonInput.OnButtonHeld( button.Code );
            } else {
                if (button.IsHeld) {
                    buttonInput.OnButtonReleased( button.Code );
                }
                button.IsHeld = false;
            }
        }

        private void TriggerAxisUpdate(JoystickAxis axis)
        {
            float x = axis.GetAxisX();
            // Checks if input is below dead, if not it can continue, overwise it should set axis X value to 0
            if (IsBelowDead( x, axis.DeadZoneX )) {
                axis.X = 0;
                axis.XChange = false;
                triggerInput.OnTriggerDeadZone( axis );
                return;
            }
            // Setting new x value to trigger
            axis.X = x;
            // Call hold method
            triggerInput.OnTriggerHold( axis );
            // Should call hold change it when position of x changes
            axis.XChange = axis.Dx != 0;
            if (axis.XChange)
                triggerInput.OnTriggerChange( axis );
        }

        private void StickAxisUpdate(JoystickDoubleAxis axis)
        {
            float x = axis.GetAxisX();
            float y = axis.GetAxisY();
            // Checks if input is below dead, if not it can continue, overwise it should set axis values to 0
            if (IsInDead( x, axis.DeadZoneX ) && IsInDead( y, axis.DeadZoneY )) {
                axis.X = 0;
                axis.Y = 0;
                axis.XChange = false;
                axis.YChange = false;
                stickInput.OnStickDeadZone( axis );
                return;
            }
            // Setting new x and y value to axis
            axis.X = x;
            axis.Y = y;
            // Call hold method
            stickInput.OnStickHold( axis );
            // Should call hold change it when position of x or y changes
            axis.XChange = axis.Dx != 0;
            axis.YChange = axis.Dy != 0;
            if (axis.HasAnyChanged())
                stickInput.OnStickChange( axis );
        }

        private void ArrowsAxisUpdate(JoystickDoubleAxis axis)
        {
            float x = axis.GetAxisX();
            float y = axis.GetAxisY();
            // Setting new x and y value to axis
            axis.X = x;
            axis.Y = y;
            if (axis.X == 0 && axis.Y == 0) {
                arrowsInput.OnArrowsDeadZone( axis );
                return;
            }
            // Call hold method
            arrowsInput.OnArrowsHold( axis );
            // Should call hold change it when position of x or y changes
            axis.XChange = axis.Dx != 0;
            axis.YChange = axis.Dy != 0;
            if (axis.HasAnyChanged())
                arrowsInput.OnArrowsChange( axis );
        }
        
        private bool IsBelowDead(float axis, float dead)
        {
            return axis <= dead;
        }

        private bool IsInDead(float axis, float dead)
        {
            return axis <= dead && axis >= -dead;
        }
        #endregion

        #region Setters/Getters
        public IButtonListener ButtonInput { get => buttonInput; set => buttonInput = value; }
        public IStickListener StickInput { get => stickInput; set => stickInput = value; }
        public ITriggerListener TriggerInput { get => triggerInput; set => triggerInput = value; }
        public IArrowsListener ArrowInput { get => arrowsInput; set => arrowsInput = value; }
        public bool InputEnabled { get => inputEnabled; set => inputEnabled = value; }
        public bool UsesJoystick { get => usesJoystick; set => usesJoystick = value; }
        public KeyboardSettings AlternativeKeyboard { get => alternativeKeyboard; }
        public int PlayerNumber { get => playerNumber; set => playerNumber = value; }
        #endregion
    }
}