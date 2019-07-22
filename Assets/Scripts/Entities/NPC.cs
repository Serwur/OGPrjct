using DoubleMMPrjc.AI;

namespace DoubleMMPrjc
{
    public abstract class NPC : Entity, IStateAIListener
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
            if (!isDead) {
                DrawGizmos();
            }
        }

        public override void Die()
        {
            base.Die();
            ai.Stop();
        }

        public abstract void DrawGizmos();

        public void SetSleepState(AIMoveState oldState)
        {
            throw new System.NotImplementedException();
        }

        public void SetWatchState(AIMoveState oldState)
        {
            throw new System.NotImplementedException();
        }

        public void SetReachState(AIMoveState oldState)
        {
            throw new System.NotImplementedException();
        }

        public void SetAttackState(AIMoveState oldState)
        {
            throw new System.NotImplementedException();
        }

        public void SetFollowState(AIMoveState oldState)
        {
            throw new System.NotImplementedException();
        }

        public void OnAnyStateChange(AIMoveState oldState, AIMoveState newState)
        {
            throw new System.NotImplementedException();
        }

        public void SleepUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void WatchUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void ReachUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void FollowUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void AttackUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void OnAnyStateUpdate(AIMoveState currentState)
        {
            throw new System.NotImplementedException();
        }

        #region Getters and Setters

        #endregion
    }
}
