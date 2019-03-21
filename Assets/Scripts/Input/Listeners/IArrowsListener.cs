namespace Inputs
{
    public interface IArrowsListener
    {
        void OnArrowsChange(JoystickDoubleAxis arrows);
        void OnArrowsHold(JoystickDoubleAxis arrows);
        void OnArrowsDeadZone(JoystickDoubleAxis arrows);
    }
}
