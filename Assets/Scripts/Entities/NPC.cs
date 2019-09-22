using ColdCry.AI;
using ColdCry.Utility;
using UnityEngine;

namespace ColdCry.Objects
{
    public abstract class NPC : Entity, IStateAIListener
    {
        protected AIBehaviour aiBehaviour;

        #region Unity API
        public override void Awake()
        {
            base.Awake();
            aiBehaviour = GetComponent<AIBehaviour>();
        }

        public void OnDrawGizmos()
        {
            if (!IsDead) {
                DrawGizmos();
            }
        }
        #endregion
     
        public override void OnDie()
        {
            AIBehaviour.Stop();
        }

        public void SetSleepState(AIState oldState)
        {

        }

        public void SetWatchState(AIState oldState)
        {

        }

        public void SetReachState(AIState oldState)
        {

        }

        public void SetAttackState(AIState oldState)
        {

        }

        public void SetFollowState(AIState oldState)
        {

        }

        public void OnAnyStateChange(AIState oldState, AIState newState)
        {

        }

        public void OnAnyStateUpdate(AIState currentState)
        {

        }

        public void SleepUpdate()
        {

        }

        public void WatchUpdate()
        {

        }

        public void ReachUpdate()
        {

        }

        public void FollowUpdate()
        {

        }

        public void AttackUpdate()
        {

        }

        #region Abstract
        public abstract void DrawGizmos();
        #endregion

        #region Getters and Setters
        public AIBehaviour AIBehaviour { get => aiBehaviour; private set => aiBehaviour = value; }
        #endregion
    }
}
