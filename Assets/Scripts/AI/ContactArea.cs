using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        [RequireComponent( typeof( BoxCollider ) )]
        public class ContactArea : MonoBehaviour
        {
            public LayerMask nodeLayers;

            private BoxCollider coll;
            private LinkedList<Entity> entitiesIn = new LinkedList<Entity>();
            private List<Node> nodesIn = new List<Node>();

            #region Unity API
            public void Awake()
            {
                coll = GetComponent<BoxCollider>();
            }

            public void Start()
            {
                AIManager.AddContactArea( this );
                Collider[] colliders = Physics.OverlapBox( coll.bounds.center, coll.bounds.extents, coll.transform.rotation, nodeLayers, QueryTriggerInteraction.Collide );
                foreach (Collider collider in colliders) {
                    nodesIn.Add( collider.GetComponent<Node>() );
                }
            }

            public void OnTriggerEnter(Collider other)
            {
                Entity entity = other.GetComponent<Entity>();
                if (entity != null) {
                    AddEntity( entity );
                }
            }

            public void OnTriggerExit(Collider other)
            {
                Entity entity = other.GetComponent<Entity>();
                if (entity != null) {
                    RemoveEntity( entity );
                }
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Adds entity to contact entities list and adds reference to
            /// this contact area in entity
            /// </summary>
            /// <param name="entity">Entity to add</param>
            public void AddEntity(Entity entity)
            {
                entity.ContactArea = this;
                entitiesIn.AddLast( entity );
            }

            /// <summary>
            /// Removes given entity from contant entities list and removes
            /// reference of contact area from entity
            /// </summary>
            /// <param name="entity">Entity to remove</param>
            /// <returns><code>TRUE</code> if entity was removed, otherwise <code>FALSE</code></returns>
            public bool RemoveEntity(Entity entity)
            {
                if (entity.ContactArea == this) {
                    entity.ContactArea = null;
                }
                return entitiesIn.Remove( entity );
            }

            /// <summary>
            /// Checks if given entity is in contact area
            /// </summary>
            /// <param name="entity">Entity to check</param>
            /// <returns><code>TRUE</code> if entity is in contact area, otherwise <code>FALSE</code></returns>
            public bool Contains(Entity entity)
            {
                return entitiesIn.Contains( entity );
            }
            #endregion

            #region Getters And Setters
            public List<Node> Nodes { get => nodesIn; }
            #endregion
        }
    }
}