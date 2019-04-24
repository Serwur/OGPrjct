using System;
using UnityEngine;
using System.Collections.Generic;
using DoubleMMPrjc.AI;

namespace DoubleMMPrjc
{
    public class NPC : Entity
    {
        protected Vector2 currentTarget;
        protected Vector2 moveDirection;
        protected Transform followTarget;

        protected Stack<Node> currentPath = new Stack<Node>();
        protected Node currentNode = null;

        public virtual void Move(float moveSpeed)
        {
            transform.Translate( moveDirection * moveSpeed * Time.fixedDeltaTime, Space.World );
        }

        /// <summary>
        /// Metoda w której AI powinno decydować o zachowaniu i sposobie ruchu
        /// </summary>
        public virtual void UpdateMovePosition(Vector2 position)
        {
            currentTarget = position;
            moveDirection = new Vector2( currentTarget.x - transform.position.x, 0 ).normalized;
        }

        /// <summary>
        /// Gives next node to follow in AI path
        /// </summary>
        public virtual void NextNodeInPath()
        {
            if (currentPath == null) return;
            if ( currentPath.Count > 0) {
                currentNode = currentPath.Pop();
                UpdateMovePosition( currentNode.transform.position );
            } else {
                currentPath = null;
                currentNode = null;
                followTarget = GameManager.Character.transform;
                UpdateMovePosition( followTarget.position );
            }
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

        public Node CurrentNode { get => currentNode;}
    }
}
