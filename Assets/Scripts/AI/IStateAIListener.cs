namespace ColdCry.AI
{
    public interface IStateAIListener
    {
        void SetSleepState(AIState oldState);
        void SetWatchState(AIState oldState);
        void SetReachState(AIState oldState);
        void SetAttackState(AIState oldState);
        void SetFollowState(AIState oldState);
        void OnAnyStateChange(AIState oldState, AIState newState);

        void SleepUpdate();
        void WatchUpdate();
        void ReachUpdate();
        void FollowUpdate();
        void AttackUpdate();
        void OnAnyStateUpdate(AIState currentState);
    }
}
