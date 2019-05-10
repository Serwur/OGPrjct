using UnityEngine;

namespace DoubleMMPrjc
{

    public class Dummy : NPC
    {
       
        public override void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            canMove = false;
            isPaused = true;
            isPushImmune = true;
            isInviolability = true;
        }

        public override void Start()
        {
            // ZERO ACTIONS
        }

        public override void FixedUpdate()
        {
            // ZERO ACTIONS
        }

        public override void OnAnyStateUpdate()
        {
            // ZERO ACTIONS
        }

    }

}
