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

            public void AddEntity(Entity entity)
            {
                entity.ContactArea = this;
                entitiesIn.AddLast( entity );
            }

            public void RemoveEntity(Entity entity)
            {
                if (entity.ContactArea == this) {
                    entity.ContactArea = null;
                }
                entitiesIn.Remove( entity );
            }

            public bool Contains(Entity entity)
            {
                return entitiesIn.Contains( entity );
            }

            public List<Node> Nodes { get => nodesIn; }

        }
    }
}