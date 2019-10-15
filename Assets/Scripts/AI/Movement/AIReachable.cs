using ColdCry.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.AI.Movement
{
    public class AIReachable : MonoBehaviour
    {
        [SerializeField] private ContactArea contactArea;

        public bool AddAIFollower(AIMovementBehaviour follower)
        {
            return Followers.Add( follower );
        }

        public bool RemoveAIFollower(AIMovementBehaviour follower)
        {
            return Followers.Remove( follower );
        }

        /*
        private void NotifyAIFollower()
        {
            foreach (AIMovementBehaviour follower in Collections.ToArray( Followers )) {
                AIMovementResponse response = follower.TrackTarget( this, false );
                switch (response) {
                    case AIMovementResponse.NO_CONTACT_AREA:
                    case AIMovementResponse.NO_PATH_TO_TARGET:
                        follower.StartPathRefind();
                        break;
                }
            }
        }*/

        public ContactArea ContactArea { get => contactArea; set => contactArea = value; }
        private HashSet<AIMovementBehaviour> Followers { get; set; }
    }
}
