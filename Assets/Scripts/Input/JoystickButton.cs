using UnityEngine;

public class JoystickButton
{
    protected PlayerInput input;
    protected bool isHeld = false;
    protected string unityInput;
    protected ButtonCode code;

    public JoystickButton(PlayerInput input, string unityInput, ButtonCode code)
    {
        this.input = input;
        this.unityInput = unityInput;
        this.code = code;
    }

    public bool GetButton()
    {
        if ( !input.UsesJoystick ) {
            return Input.GetKey(input.AlternativeKeyboard.GetKey(code));
        } else {
            return Input.GetButton(unityInput);
        }
    }

    public override string ToString()
    {
        return code.ToString();
    }

    public bool IsHeld { get => isHeld;  set => isHeld = value; }
    public string UnityInput { get => unityInput; set => unityInput = value; }
    public ButtonCode Code { get => code; }
}
