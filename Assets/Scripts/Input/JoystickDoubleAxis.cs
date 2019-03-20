using UnityEngine;

public class JoystickDoubleAxis : JoystickAxis
{
    protected float y = 0;
    protected float dy = 0;
    protected float deadZoneY = 0;
    protected string unityInputY;
    protected bool yChange = false;

    public JoystickDoubleAxis(PlayerInput input, string unityInputX, string unityInputY, AxisCode code, float deadZoneX, float deadZoneY) : base( input, unityInputX, code, deadZoneX )
    {
        this.unityInputY = unityInputY;
        this.deadZoneY = deadZoneY;
    }

    protected override void SetCode(AxisCode code)
    {
        this.code = code;
        switch (code) {
            case AxisCode.LeftStick:
            case AxisCode.RightStick:
            case AxisCode.Arrows:
                break;
            default:
                throw new System.Exception( "JoystickDoubleAxis::SetCode::(Wrong type parameter, you should use " +
                                             "LeftStick, RightStick or Arrows to initialize double axis)" );
        }

    }

    public override float GetAxisX()
    {
        if (!input.UsesJoystick) {
            return input.AlternativeKeyboard.GetKeyX( code );
        } else {
            return Input.GetAxis( unityInputX );
        }
    }

    public float GetAxisY()
    {
        if (!input.UsesJoystick) {
            return input.AlternativeKeyboard.GetKeyY( code );
        } else {
            return Input.GetAxis( unityInputY );
        }
    }

    public override string ToString()
    {
        return base.ToString() + ", y = " + y + ", dy = " + dy;
    }

    public bool HasAnyChanged()
    {
        if (XChange)
            return true;
        if (YChange)
            return true;
        return false;
    }

    public float Y
    {
        get => y;
        set {
            dy = value - y;
            y = value;
        }
    }

    public float Dy { get => dy; }
    public float DeadZoneY { get => deadZoneY; set => deadZoneY = value; }
    public string UnityInputY { get => unityInputY; set => unityInputY = value; }
    public bool YChange { get => yChange; set => yChange = value; }
}
