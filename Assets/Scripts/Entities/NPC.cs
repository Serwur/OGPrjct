using DoubleMMPrjc.AI;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    public class NPC : Entity
    {
        protected Vector2 moveDirection;
        protected Transform followingTarget;

        protected Stack<Node> currentPath = null;
        protected Node currentNode = null;
        protected Node previousNode = null;

        /// <summary>
        /// Moves towards current target
        /// </summary>
        /// <param name="moveSpeed"></param>
        public virtual void Move(float moveSpeed)
        {
            transform.Translate( moveDirection * moveSpeed * Time.fixedDeltaTime, Space.World );
        }

        public virtual void FindPath(Entity target)
        {
            ResetPath();
            currentPath = AIManager.FindPath( this, target );
        }

        /// <summary>
        /// Updates move direction depends on current target
        /// </summary>
        public virtual void UpdateMoveDirection()
        {
            UpdateMoveDirection( followingTarget.transform.position );
        }

        /// <summary>
        /// Updates move direction depends on given position. It only takes x value from vector
        /// </summary>
        /// <param name="position">Position</param>
        public virtual void UpdateMoveDirection(Vector2 position)
        {
            UpdateMoveDirection( position.x );
        }

        /// <summary>
        /// Update move direction depdens on given parameter, x > 0 gives 1, other values gives -1
        /// </summary>
        /// <param name="x">X value used for move direction vector</param>
        public virtual void UpdateMoveDirection(float x)
        {
            moveDirection = new Vector2( ( x > 0 ? 1 : -1 ), 0 );
        }

        /// <summary>
        /// Gives next node to follow in AI path
        /// </summary>
        public virtual void NextNodeInPath()
        {
            if (currentPath == null)
                return;
            if (currentPath.Count > 0) {
                previousNode = currentNode;
                currentNode = currentPath.Pop();
                UpdateMoveDirection( currentNode.transform.position );
            } else {
                FollowTarget( GameManager.Character );
            }
        }

        /// <summary>
        /// Resets current path
        /// </summary>
        public virtual void ResetPath()
        {
            currentPath = null;
            currentNode = null;
            previousNode = null;
        }

        /// <summary>
        /// Start following target straight in line
        /// </summary>
        /// <param name="entity">Target to follow</param>
        public virtual void FollowTarget(Entity entity)
        {
            ResetPath();
            UpdateMoveDirection( followingTarget.position );
        }

        /// <summary>
        /// Checks if given node is in AI path
        /// </summary>
        /// <param name="node">Node to check</param>
        /// <returns><code>TRUE</code> if node is in path, otherwise <code>FALSE</code></returns>
        public bool IsNodeInPath(Node node)
        {
            if (currentPath == null)
                return false;
            return currentPath.Contains( node );
        }

        /// <summary>
        /// Checks if AI has a path to follow
        /// </summary>
        /// <returns><code>TRUE</code> if AI has path, otherwise <code>FALSE</code></returns>
        public bool HasPath()
        {
            return currentPath != null;
        }


        protected void Jump(Node node)
        {
            float jumpDirection = node.transform.position.x - transform.position.x;

        }

        protected void Jump(float jumpPower, float direction)
        {
            rb.velocity = new Vector2( rb.velocity.x, jumpPower );
            lastMinFallSpeed = 0;
        }

        public Node CurrentNode { get => currentNode; }
    }
}
