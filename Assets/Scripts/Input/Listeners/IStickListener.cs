namespace Inputs
{
    /// <summary>
    /// Listener for left and right sticks axis.
    /// </summary>
    public interface IStickListener
    {
        /// <summary>
        /// Is called every frame when left or right stick position is changed
        /// </summary>
        /// <param name="stick">Information about changed stick</param>
        void OnStickChange(JoystickDoubleAxis stick);
        /// <summary>
        /// Is called every frame when left or right stick is not in dead zone
        /// </summary>
        /// <param name="stick">Information about moved stick</param>
        void OnStickHold(JoystickDoubleAxis stick);
        /// <summary>
        /// Is called every frame when left or right stick position is in dead zone
        /// </summary>
        /// <param name="stick">Information about stick that is in dead zone</param>
        void OnStickDeadZone(JoystickDoubleAxis stick);
    }
}

