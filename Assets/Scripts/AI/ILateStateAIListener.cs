namespace ColdCry.AI
{
    public interface ILateStateAIListener
    {
        void SetLateSleepState(AIState oldState);
        void SetLateWatchState(AIState oldState);
        void SetLateReachState(AIState oldState);
        void SetLateAttackState(AIState oldState);
        void SetLateFollowState(AIState oldState);
        void OnAnyLateStateChange(AIState oldState, AIState newState);

        void LateSleepUpdate();
        void LateWatchUpdate();
        void LateReachUpdate();
        void LateFollowUpdate();
        void LateAttackUpdate();
        void OnAnyLateStateUpdate(AIState currentState);
    }
}