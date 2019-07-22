using DoubleMMPrjc.AI;
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
            canMove = false;
            isPaused = true;
            isPushImmune = true;
            isInviolability = true;
        }

        public override void Start()
        {
            // IT'S JUST DUMMY
        }

        public override void FixedUpdate()
        {
            // IT'S JUST DUMMY
        }

        public override void OnContactAreaEnter(ContactArea contactArea)
        {
            // IT'S JUST DUMMY
        }

        public override void OnContactAreaExit(ContactArea contactArea)
        {
            // IT'S JUST DUMMY
        }

        public override void DrawGizmos()
        {
           
        }
    }

}
