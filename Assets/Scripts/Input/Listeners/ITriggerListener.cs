namespace Inputs
{
    public interface ITriggerListener
    {
        void OnTriggerChange(JoystickAxis trigger);
        void OnTriggerHold(JoystickAxis trigger);
        void OnTriggerDeadZone(JoystickAxis trigger);
    }
}


