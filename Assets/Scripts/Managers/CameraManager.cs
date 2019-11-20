﻿using UnityEngine;

namespace ColdCry.Core
{
    public class CameraManager : MonoBehaviour
    {
        public bool isFollowingTarget = true;


        private Transform targetToFollow;
        private bool slowTargetChange = false;
        private float speedTargetChange = 0f;

        #region DevSection
        public bool mouseControl = false;
        #endregion

        private static CameraManager Instance;
        private static readonly Vector3 OFFSET_Z = new Vector3( 0f, 0f, -20f );

        public void Awake()
        {
            if (Instance != null) {
                Debug.LogError( "CameraManager::Awake::(Trying to create more than one camera manager!)" );
                return;
            }
            Instance = this;
        }

        public void Update()
        {
            if (mouseControl) {
                if (Input.GetKey( KeyCode.Mouse2 )) {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                    Vector2 deltaMove = mousePos - Camera.main.transform.position;
                    Camera.main.transform.position = Camera.main.transform.position + new Vector3( deltaMove.x, deltaMove.y ) * 0.2f;
                }
            } else if (isFollowingTarget) {
                transform.position = targetToFollow.position + OFFSET_Z;
            } else if (slowTargetChange) {
                transform.position = Vector3.LerpUnclamped( transform.position, targetToFollow.position + OFFSET_Z, speedTargetChange * Time.deltaTime );
                if (Vector2.Distance( transform.position, targetToFollow.position ) < 0.35f) {
                    slowTargetChange = false;
                    isFollowingTarget = true;
                }
            }
        }

        public static bool Exists()
        {
            return Instance != null;
        }

        public static void FollowPlayer()
        {
            FollowTarget( GameManager.Player.transform );
        }

        public static void FollowPlayer(float speedTargetChange)
        {
            FollowTarget( GameManager.Player.transform, speedTargetChange );
        }

        public static void FollowTarget(Transform transform)
        {
            Instance.targetToFollow = transform;
            Instance.slowTargetChange = false;
            Instance.isFollowingTarget = true;
        }

        public static void FollowTarget(Transform transform, float speedTargetChange)
        {
            Instance.targetToFollow = transform;
            Instance.speedTargetChange = Mathf.Clamp( speedTargetChange, 0.01f, 1f ) * 7f;
            IsFollowingTarget = false;
            Instance.slowTargetChange = true;
        }

        public static bool IsFollowingTarget { get => Instance.isFollowingTarget; set => Instance.isFollowingTarget = value; }
    }
}