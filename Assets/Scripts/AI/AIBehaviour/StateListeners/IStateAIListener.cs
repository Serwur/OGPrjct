namespace DoubleMMPrjc
{
    namespace AI
    {
        public interface IStateAIListener
        {
            void SetSleepState(AIMoveState oldState);
            void SetWatchState(AIMoveState oldState);
            void SetReachState(AIMoveState oldState);
            void SetAttackState(AIMoveState oldState);
            void SetFollowState(AIMoveState oldState);
            void OnAnyStateChange(AIMoveState oldState, AIMoveState newState);

            void SleepUpdate();
            void WatchUpdate();
            void ReachUpdate();
            void FollowUpdate();
            void AttackUpdate();
            void OnAnyStateUpdate(AIMoveState currentState);
        }
    }
}