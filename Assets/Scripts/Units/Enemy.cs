using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    /// <summary>
    /// Metoda w której AI powinno decydować o zachowaniu i sposobie ruchu
    /// </summary>
    public virtual void Movement()
    {

    }

    /// <summary>
    /// Metoda w której AI powinno decydować o sposobie ataku
    /// </summary>
    public virtual void Atack()
    {

    }

    public override void Die()
    {
    }

    public override void ResetUnit()
    {
    }

    public override void UpdateAttributes()
    {
    }
}
