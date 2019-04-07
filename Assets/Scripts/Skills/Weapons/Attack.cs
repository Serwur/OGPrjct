public class Attack
{
    public readonly float damage = 1f;

    public Attack(float damage)
    {
        this.damage = damage;
    }

    public static Attack One { get => new Attack( 1 ); }
}
