using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Range( 0.1f, 5f )] public float damage = 1.0f;

    private Character character;
    private Attack attack;

    public void Start()
    {
        character = (Character) GameManager.GetEntityByName( "Player" );
    }

    public void OnTriggerEnter(Collider other)
    {
        if (attack != null && other.CompareTag( "Enemy" )) {
            other.GetComponent<Entity>().TakeDamage( attack );
        }
    }

    public void SetNextAttackInfo(Attack attack)
    {
        this.attack = attack;
    }
}
