using ColdCry.AI.Movement;
using ColdCry.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.AI
{
    [RequireComponent( typeof( BoxCollider ) )]
    public class ContactArea : MonoBehaviour
    {
        public LayerMask nodeLayers;

        private BoxCollider coll;
        private LinkedList<Reachable> reachablesInArea = new LinkedList<Reachable>();
        private HashSet<Node> nodesIn = new HashSet<Node>();

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
            Reachable reachable = other.GetComponent<Reachable>();
            if (reachable != null) {
                AddReachable( reachable );
                reachable.NoticeContactAreaEnter( this );
            }
        }

        public void OnTriggerExit(Collider other)
        {
            Reachable reachable = other.GetComponent<Reachable>();
            if ( reachable != null ) {
                reachable.NoticeContactAreaExit( this );
                RemoveReachable( reachable );
            }         
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds entity to contact entities list and adds reference to
        /// this contact area in entity
        /// </summary>
        /// <param name="reachable">AIReachable to add</param>
        public void AddReachable(Reachable reachable)
        {
            reachablesInArea.AddLast( reachable );
        }

        /// <summary>
        /// Removes given entity from contant entities list and removes
        /// reference of contact area from entity
        /// </summary>
        /// <param name="reachable">AIReachable to remove</param>
        /// <returns>True if entity was removed, false otherwise</code></returns>
        public bool RemoveReachable(Reachable reachable)
        {
            return reachablesInArea.Remove( reachable );
        }

        /// <summary>
        /// Checks if given entity is in contact area
        /// </summary>
        /// <param name="entity">AIReachable to check</param>
        /// <returns>True if entity is in contact area, false otherwise</code></returns>
        public bool Contains(Reachable entity)
        {
            return reachablesInArea.Contains( entity );
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
        public HashSet<Node> Nodes { get => nodesIn; }
        #endregion
    }
}