using DoubleMMPrjc.AI;
using DoubleMMPrjc.Timer;
using UnityEngine;

namespace DoubleMMPrjc
{
    public abstract class NPC : Entity
    {

        public AIBehaviour ai;

        #region Unity API
        public override void Awake()
        {
            base.Awake();
            ai = GetComponent<AIBehaviour>();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        #endregion

     
        

        public void OnDrawGizmos()
        {
            if ( !isDead ) {
                DrawGizmos();
            }
        }

       

        public override void Die()
        {
            base.Die();
            ai.Stop();
        }

        public abstract void DrawGizmos();

        #region Getters and Setters
        /// <summary>
        /// <code>TRUE</code> if AI has path, otherwise <code>FALSE</code>
        /// </summary>
        public bool HasPath { get => !currentPath.IsFullyEmpty; }
        /// <summary>
        /// <code>TRUE</code> if AI path has ended, otherwise <code>FALSE</code>
        /// </summary>
        public bool PathHasEnded { get => currentPath.IsEmpty; }
        public ComplexNode CurrentComplexNode { get => currentCn; }
        /// <summary>
        /// State of enemy's AI
        /// </summary>
        public AIState State { get => state; }
        public AIMovingState MovingState { get => movingState; }
        #endregion
    }
}
 