using ColdCry.Exception;
using ColdCry.Objects;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa, która będzie posiadała podstawowe informacje o wszystkich statycznych oraz dynamicznych obiektach</br>
/// znajdujących się na poziomie. Powinna ona posiadać metody ułatwiające komunikację z graczami, a obiektami</br>
/// na których mogą wykonywać interakcje.
/// </summary>
namespace ColdCry.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager Instance;

        private LinkedList<Entity> entities = new LinkedList<Entity>();
        private Character player;

        [SerializeField] private bool drawEnemyRange = true;
        [SerializeField] private bool drawAIDestination = true;
        [SerializeField] private bool drawNodeConnections = true;

        private void Awake()
        {
            if (Instance != null) {
                Debug.LogError( "GameMaster::Awake::(Trying to create another GameMaster object!)" );
                Destroy( gameObject );
            }
            Instance = this;
        }

        private void Start()
        {
            Character [] players = GameObject.FindObjectsOfType<Character>();
            
            if ( players.Length == 0) {
                throw new MissingEssentialGameObjectException( "Missing game object of type Character on current scene" );
            } 

            if ( players.Length > 1) {
                throw new MissingEssentialGameObjectException("Current state of program doesn't allow these amount of players on scene: " + players.Length);
            }

            player = players[0];

            if ( CameraManager.Exists() ) {
                CameraManager.FollowPlayer();
            }
        }

        /// <summary>
        /// Adds an entity to the list. It should be called whenever a new entity is created on scene.
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public static void AddEntity(Entity entity)
        {
            Instance.entities.AddLast( entity );
        }

        /// <summary>
        /// Removes an entity. It should be called whenever entity is removed from scene.
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        /// <returns>TRUE if given Entity exists in the list, otherwise FALSE</returns>
        public static bool RemoveEntity(Entity entity)
        {
            return Instance.entities.Remove( entity );
        }

        /// <summary>
        /// Pause/unpause all entities on scene
        /// </summary>
        /// <param name="paused">If TRUE entites will be paused, otherwise FALSE</param>
        public static void PauseEntities(bool paused)
        {
            foreach (Entity entity in Instance.entities) {
                entity.IsPaused = paused;
            }
        }

        /// <summary>
        /// Resets all the existing entities
        /// </summary>
        public static void ResetAllEntities()
        {
            foreach (Entity entity in Instance.entities)
                entity.ResetUnit();
        }

        /// <summary>
        /// Gets an entity from scene by given name. It's a bit faster than entity gets scripts.
        /// </summary>
        /// <param name="name">Name of entity (as object in scene)</param>
        /// <returns>Entity by given name, if it doesn't exists then NULL is returned</returns>
        public static Entity GetEntityByName(string name)
        {
            foreach (Entity entity in Instance.entities) {
                if (entity.gameObject.name.Equals( name ))
                    return entity;
            }
            return null;
        }

        /// <summary>
        /// Gets all the entities from scene
        /// </summary>
        /// <returns>Entities array from scene</returns>
        public static Entity[] GetEntities()
        {
            Entity[] entities = new Entity[Instance.entities.Count];
            Instance.entities.CopyTo( entities, 0 );
            return entities;
        }

        public static Character Player { get => Instance.player; }
        public static bool DrawEnemyRange { get => Instance != null && Instance.drawEnemyRange; }
        public static bool DrawAIDestination { get => Instance != null && Instance.drawAIDestination; }
        public static bool DrawNodeConnections { get => Instance != null && Instance.drawNodeConnections; }
    }
}