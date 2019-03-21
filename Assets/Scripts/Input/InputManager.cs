using UnityEngine;

namespace Inputs
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance;

        private string[] joysticks;

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
            else {
                Debug.LogError( "InputManager::Awake::(Trying to create another InputManager object!)" );
                Destroy( gameObject );
                return;
            }

            joysticks = Input.GetJoystickNames();
        }

        public void Start()
        {
            
        }

        public void Update()
        {
            
        }
    }
}


