using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa, która będzie posiadała podstawowe informacje o wszystkich statycznych oraz dynamicznych obiektach</br>
/// znajdujących się na poziomie. Powinna ona posiadać metody ułatwiające komunikację z graczami, a obiektami</br>
/// na których mogą wykonywać interakcje.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager Instance;

    private LinkedList<Entity> entities = new LinkedList<Entity>();

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError( "GameMaster::Awake::(Trying to create another GameMaster object!)" );
            Destroy( gameObject );
        }
        Instance = this;
    }

    public static void AddEntity(Entity entity)
    {
        Instance.entities.AddLast( entity );
    }

    public static bool RemoveEntity(Entity entity)
    {
        return Instance.entities.Remove( entity );
    }

    public static void PauseEntities(bool paused)
    {
        foreach (Entity entity in Instance.entities) {
            entity.IsPaused = paused;
        }
    }

    public static Entity GetEntityByName(string name)
    {
        foreach (Entity entity in Instance.entities) {
            if (entity.gameObject.name.Equals( name ))
                return entity;
        }
        return null;
    }
}
