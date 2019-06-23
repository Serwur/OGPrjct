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
                    collider.enabled = false;
                }
            }

            public void OnTriggerEnter(Collider other)
            {
                Entity entity = other.GetComponent<Entity>();
                if (entity != null) {
                    AddEntity( entity );
                    entity.OnContactAreaEnter( this );
                }
            }

            public void OnTriggerExit(Collider other)
            {
                Entity entity = other.GetComponent<Entity>();
                if (entity != null) {
                    RemoveEntity( entity );
                    entity.OnContactAreaExit( this );
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
            /// <returns>True if entity was removed, false otherwise</code></returns>
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
            /// <returns>True if entity is in contact area, false otherwise</code></returns>
            public bool Contains(Entity entity)
            {
                return entitiesIn.Contains( entity );
            }

            /// <summary>
            /// Checks if <see cref="ContactArea"/> contains given <see cref="Node"/>
            /// </summary>
            /// <param name="node"><see cref="Node"/> to check</param>
            /// <returns>True if contains, false otherwise</returns>
            public bool Contains(Node node)
            {
                return nodesIn.Contains( node );
            }

            /// <summary>
            /// Gets random <b>x</b> position from bounds of area
            /// </summary>
            /// <returns>Random <b>x</b> from area</returns>
            public float GetRandXInArea()
            {
                float maxX = coll.bounds.center.x + coll.bounds.extents.x;
                float minX = coll.bounds.center.x - coll.bounds.extents.x;
                return Random.Range( minX, maxX );
            }

            /// <summary>
            /// Gets random <b>y</b> position from bounds of area
            /// </summary>
            /// <returns>Random <b>y</b> from area</returns>
            public float GetRandYInArea()
            {
                float maxY = coll.bounds.center.y + coll.bounds.extents.y;
                float minY = coll.bounds.center.y - coll.bounds.extents.y;
                return Random.Range( minY, maxY );
            }

            /// <summary>
            /// Gets random <see cref="Vector2"/> from bounds of area
            /// </summary>
            /// <returns><see cref="Vector2"/> from area</returns>
            public Vector2 GetRandPosInArea()
            {
                return new Vector2( GetRandXInArea(), GetRandYInArea() );
            }
            #endregion

            #region Getters And Setters
            public List<Node> Nodes { get => nodesIn; }
            #endregion
        }
    }
}