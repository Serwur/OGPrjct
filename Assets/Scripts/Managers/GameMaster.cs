using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa, która będzie posiadała podstawowe informacje o wszystkich statycznych oraz dynamicznych obiektach</br>
/// znajdujących się na poziomie. Powinna ona posiadać metody ułatwiające komunikację z graczami, a obiektami</br>
/// na których mogą wykonywać interakcje.
/// </summary>
public class GameMaster : MonoBehaviour
{
    // SINGLETON
    public static GameMaster Instance;

    private HashSet<IInteractable> interactables = new HashSet<IInteractable>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else {
            Debug.LogError( "GameMaster::Awake::(Trying to create another GameMaster object!)" );
            Destroy( gameObject );
            return;
        }
        
    }

    public IInteractable NearestInteractable(Vector2 position)
    {
        float minDistance = float.MaxValue;
        IInteractable nearest = null;
        foreach (IInteractable interactable in interactables) {
        }
        return nearest;
    }

    public HashSet<IInteractable> Interactables { get => interactables; }
}
