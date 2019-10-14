using ColdCry.AI;
using ColdCry.Utility;
using UnityEngine;

namespace ColdCry.Objects
{
    public class NPC : Entity
    {
        protected AIMovementBehaviour aiBehaviour;

        #region Unity API
        public override void Awake()
        {
            base.Awake();
            aiBehaviour = GetComponent<AIMovementBehaviour>();
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

        #region Abstract


        public override void OnMove(float moveSpeed, float x)
        {
            //throw new System.NotImplementedException();
        }

        public override void OnJump(float jumpPower)
        {
            //throw new System.NotImplementedException();
        }

        public override void OnFallen(float speedWhenFalling)
        {
     //       throw new System.NotImplementedException();
        }

        public override void OnDamageTook(Attack attack)
        {
   //         throw new System.NotImplementedException();
        }

        public override void OnRegenerate()
        {
     //       throw new System.NotImplementedException();
        }

        public override void OnPushedOff(float pushPower, Vector3 direction, float disableTime)
        {
           // throw new System.NotImplementedException();
        }

        public override void OnBeingHealed(float healedHp)
        {
          //  throw new System.NotImplementedException();
        }

        public override void DrawGizmos()
        {
          //  throw new System.NotImplementedException();
        }
        #endregion

        #region Getters and Setters
        public AIMovementBehaviour AIBehaviour { get => aiBehaviour; private set => aiBehaviour = value; }
        #endregion
    }
}
