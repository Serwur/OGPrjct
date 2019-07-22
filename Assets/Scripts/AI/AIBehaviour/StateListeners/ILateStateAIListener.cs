namespace DoubleMMPrjc
{
    namespace AI
    {
        public interface ILateStateAIListener
        {
            void SetLateSleepState(AIMoveState oldState);
            void SetLateWatchState(AIMoveState oldState);
            void SetLateReachState(AIMoveState oldState);
            void SetLateAttackState(AIMoveState oldState);
            void SetLateFollowState(AIMoveState oldState);
            void OnAnyLateStateChange(AIMoveState oldState, AIMoveState newState);

            void LateSleepUpdate();
            void LateWatchUpdate();
            void LateReachUpdate();
            void LateFollowUpdate();
            void LateAttackUpdate();
            void OnAnyLateStateUpdate(AIMoveState currentState);
        }
    }
}