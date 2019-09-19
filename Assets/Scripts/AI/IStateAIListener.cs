namespace ColdCry.AI
{
    public interface IStateAIListener
    {
        void SetSleepState(AIMovementStatus oldState);
        void SetWatchState(AIMovementStatus oldState);
        void SetReachState(AIMovementStatus oldState);
        void SetAttackState(AIMovementStatus oldState);
        void SetFollowState(AIMovementStatus oldState);
        void OnAnyStateChange(AIMovementStatus oldState, AIMovementStatus newState);

        void SleepUpdate();
        void WatchUpdate();
        void ReachUpdate();
        void FollowUpdate();
        void AttackUpdate();
        void OnAnyStateUpdate(AIMovementStatus currentState);
    }
}
