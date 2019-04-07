using UnityEngine;

public class Attack
{
    public readonly float damage;
    public readonly Vector3 direction;

    public Attack(float damage, Vector3 direction)
    {
        this.damage = damage;
        this.direction = direction;
    }
}
