namespace Inputs
{
    /// <summary>
    /// Listener for left and right sticks axis.
    /// </summary>
    public interface IStickListener
    {
        void OnStickChange(JoystickDoubleAxis stick);
        void OnStickHold(JoystickDoubleAxis stick);
        void OnStickDeadZone(JoystickDoubleAxis stick);
    }
}

