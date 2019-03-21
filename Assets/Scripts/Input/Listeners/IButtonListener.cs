namespace Inputs
{
    /// <summary>
    /// Listener for following joystick buttons: A, B, X, Y, Start, Back, LeftStick, RightStick, RightBumper, LeftBumper.
    /// </summary>
    public interface IButtonListener
    {
        /// <summary>
        /// Is called once in current fixed frame when user press any of joystick buttons.
        /// </summary>
        /// <param name="code">Button code for example: ButtonCode.A, ButtonCode.RightStick etc..
        /// Use it when to know what button user has pressed.</param>
        void OnButtonPressed(ButtonCode code);

        /// <summary>
        /// Is called once in current fixed frame when user release holding joystick button.
        /// </summary>
        /// <param name="code">Button code for example: ButtonCode.A, ButtonCode.RightStick etc..
        /// Use it when to know what button user has released.</param>
        void OnButtonReleased(ButtonCode code);

        /// <summary>
        /// Is calling every fixed frame when user holds any of joystick buttons.
        /// </summary>
        /// <param name="code">Button code for example: ButtonCode.A, ButtonCode.RightStick etc..
        /// Use it when to know what button user is holding.</param>
        void OnButtonHeld(ButtonCode code);
    }
}
