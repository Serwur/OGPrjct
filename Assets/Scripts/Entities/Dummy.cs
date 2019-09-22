using ColdCry.AI;
using UnityEngine;

namespace ColdCry.Objects
{
    public class Dummy : NPC
    {
       
        public override void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Rb.freezeRotation = true;
            Rb.useGravity = false;
            CanMove = false;
            IsPaused = true;
            IsPushImmune = true;
            IsInviolability = true;
        }

        public override void Start()
        {
            // IT'S JUST DUMMY
        }

        public override void FixedUpdate()
        {
            // IT'S JUST DUMMY
        }

        public override void DrawGizmos()
        {
           
        }

        public override void OnMove(float moveSpeed, float x)
        {

        }

        public override void OnJump(float jumpPower)
        {

        }

        public override void OnFallen(float speedWhenFalling)
        {

        }

        public override void OnDamageTook(Attack attack)
        {

        }

        public override void OnRegenerate()
        {

        }

        public override void OnPushedOff(float pushPower, Vector3 direction, float disableTime)
        {

        }

        public override void OnBeingHealed(float healedHp)
        {

        }
    }

}
