using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour, PlayerInput.IButtonListener, PlayerInput.IStickListener
{
    public void OnButtonHeld(ButtonCode code)
    {
        Debug.Log(code);
    }

    public void OnButtonPressed(ButtonCode code)
    {

    }

    public void OnButtonReleased(ButtonCode code)
    {

    }

    public void OnDeadZone(JoystickDoubleAxis stick)
    {

    }

    public void OnStickChange(JoystickDoubleAxis stick)
    {
        
    }

    public void OnStickHold(JoystickDoubleAxis stick)
    {
        Debug.Log( stick );
    }
}
