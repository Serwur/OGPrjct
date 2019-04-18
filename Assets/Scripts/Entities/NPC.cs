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

        public virtual void NextNodeInPath()
        {
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

        public bool IsNodeInPath(Node node)
        {
            if (currentPath == null)
                return false;
            return currentPath.Contains( node );
        }

        public bool HasPath()
        {
            return currentPath != null;
        }

        public Node CurrentNode { get => currentNode;}
    }
}
