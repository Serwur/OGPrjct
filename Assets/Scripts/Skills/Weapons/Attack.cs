using UnityEngine;

public class Attack
{
    public readonly float damage;
    public readonly Vector3 direction;
    public readonly float pushPower;
    public readonly float pushDisableTime;

    public Attack(float damage, Vector3 direction, float pushPower) : this( damage, direction, pushPower, 0 )
    {
    }

    public Attack(float damage, Vector3 direction, float pushPower, float pushDisableTime)
    {
        this.damage = damage;
        this.direction = direction;
        this.pushPower = pushPower;
        this.pushDisableTime = pushDisableTime;
    }
}
