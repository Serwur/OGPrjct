namespace ColdCry.AI
{
    public interface ILateStateAIListener
    {
        void SetLateSleepState(AIMovementStatus oldState);
        void SetLateWatchState(AIMovementStatus oldState);
        void SetLateReachState(AIMovementStatus oldState);
        void SetLateAttackState(AIMovementStatus oldState);
        void SetLateFollowState(AIMovementStatus oldState);
        void OnAnyLateStateChange(AIMovementStatus oldState, AIMovementStatus newState);

        void LateSleepUpdate();
        void LateWatchUpdate();
        void LateReachUpdate();
        void LateFollowUpdate();
        void LateAttackUpdate();
        void OnAnyLateStateUpdate(AIMovementStatus currentState);
    }
}