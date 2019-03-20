using UnityEngine;

/// <summary>
/// Contains informations about Left and Right Triggers such as: x, dx, deadZoneX and which trigger it is.
/// </summary>
public class JoystickAxis
{
    protected PlayerInput input;
    protected string unityInputX;
    protected float x = 0;
    protected float dx = 0;
    protected float deadZoneX = 0;
    protected AxisCode code;
    protected bool xChange = false;

    /// <summary>
    /// Class is used as help for <code>PlayerInput</code> class to better organisation with passing<br>
    /// arguments to user by AxisCodeerface methods such as <code>TriggerInput</code>.
    /// Use it only, if you know what it should do.</br>
    /// </summary>
    /// <param name="unityInputX">Name of input project settings, make sure that string exists</param>
    /// <param name="code">code of axis, for single axis codes it can be only: LEFT_TRIGGER and RIGHT_TRIGGER that this class contains, so pass them to avoid exception</param>
    /// <param name="deadZoneX">Deadzone of X axis</param>
    public JoystickAxis(PlayerInput input, string unityInputX, AxisCode code, float deadZoneX)
    {
        this.input = input;
        SetCode( code );
        this.unityInputX = unityInputX;
        this.deadZoneX = deadZoneX;
    }

    /// <summary>
    /// Gets X value position of joystick axis or keyboard button in the dependency that is currently being used
    /// </summary>
    /// <returns>Position of X value</returns>
    public virtual float GetAxisX()
    {
        if (!input.UsesJoystick) {
            return Input.GetKey( input.AlternativeKeyboard.GetKey( code ) ) ? 1 : 0;
        } else {
            return Input.GetAxis(unityInputX);
        }
    }

    protected virtual void SetCode(AxisCode code)
    {
        // When user pass wrong argument then it throws excpetion
        this.code = code;
        switch (code) {
            case AxisCode.LeftTrigger:
            case AxisCode.RightTrigger:
                break;
            default:
                throw new System.Exception( "JoystickAxis::SetCode::(Wrong code parameter, you should use " +
                                     "LeftTrigger or RightTrigger to initialize single axis)" );
        }
    }

    public override string ToString()
    {
        return code.ToString() + ", x = " + x + ", dx = " + dx;
    }

    /// <summary>
    /// DO NOT SET THIS VALUE BY YOURSELF
    /// </summary>
    public float X
    {
        get => x;
        set {
            dx = value - x;
            x = value;
        }
    }

    /// <summary>
    /// CHANGE THIS VALUE FOR YOUR OWN RISK
    /// </summary>
    public string UnityInputX { get => unityInputX; set => unityInputX = value; }
    public AxisCode Code { get => code; }
    public float Dx { get => dx; }
    /// <summary>
    /// CHANGE THIS VALUE FOR YOUR OWN RISK
    /// </summary>
    public float DeadZoneX { get => deadZoneX; set => deadZoneX = value; }
    public bool XChange { get => xChange; set => xChange = value; }
}
