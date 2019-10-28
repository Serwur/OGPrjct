using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.AI.Movement
{
    public class Reachable : MonoBehaviour
    {
        [SerializeField] private ContactArea contactArea;

        private bool isFollowersIterating = false;
        private LinkedList<AIMovementBehaviour> followersToRemove = new LinkedList<AIMovementBehaviour>();

        private void Awake()
        {
            foreach (IContactable reachable in GetComponents<IContactable>()) {
                Reachables.Add( reachable );
            }
        }

        public bool AddAIFollower(AIMovementBehaviour follower)
        {
            if (follower == this) {
                throw new ArgumentException( "Follower cannot be the same object" );
            }
            return Followers.Add( follower );
        }

        public bool RemoveAIFollower(AIMovementBehaviour follower)
        {
            if (Followers.Contains( follower )) {
                if (isFollowersIterating == false) {
                    return Followers.Remove( follower );
                }
                followersToRemove.AddLast( follower );
                return true;
            }
            return false;
        }

        public void NoticeContactAreaEnter(ContactArea contactArea)
        {
            ContactArea = contactArea;

            // handling ai movement behaviour
            isFollowersIterating = true;
            foreach (AIMovementBehaviour follower in Followers) {
                follower.OnFollowedObjectAreaEnter( contactArea, this );
            }
            isFollowersIterating = false;

            foreach (IContactable reachable in Reachables) {
                reachable.OnContactAreaEnter( contactArea );
            }

            if (followersToRemove.Count > 0) {
                foreach (AIMovementBehaviour follower in followersToRemove) {
                    Followers.Remove( follower );
                }
                followersToRemove.Clear();
            }
        }

        public void NoticeContactAreaExit(ContactArea contactArea)
        {
            if (ContactArea == contactArea) {
                ContactArea = null;
            }

            // handling ai movement behaviour
            foreach (AIMovementBehaviour follower in Followers) {
                follower.OnFollowedObjectAreaExit( contactArea, this );
            }

            foreach (IContactable reachable in Reachables) {
                reachable.OnContactAreaExit( contactArea );
            }
        }

        public void OnDestroy()
        {
            // Remove this entity from contact area coz it doesn't exists anymore
            if (ContactArea != null) {
                ContactArea.RemoveReachable( this );
            }
        }

        public ContactArea ContactArea { get => contactArea; private set => contactArea = value; }
        public HashSet<AIMovementBehaviour> Followers { get; private set; } = new HashSet<AIMovementBehaviour>();
        private HashSet<IContactable> Reachables { get; set; } = new HashSet<IContactable>();
    }
}
