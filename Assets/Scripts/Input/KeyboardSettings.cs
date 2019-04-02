using UnityEngine;

namespace Inputs
{
    [CreateAssetMenu(fileName = "Keyboard Settings", menuName = "Input/Keyboard Settings")]
    public class KeyboardSettings : ScriptableObject
    {
        [SerializeField]
        private KeyCode a = KeyCode.Space;
        [SerializeField]
        private KeyCode b = KeyCode.F;
        [SerializeField]
        private KeyCode x = KeyCode.E;
        [SerializeField]
        private KeyCode y = KeyCode.Tab;
        [SerializeField]
        private KeyCode leftStick = KeyCode.Z;
        [SerializeField]
        private KeyCode rightStick = KeyCode.X;
        [SerializeField]
        private KeyCode back = KeyCode.C;
        [SerializeField]
        private KeyCode start = KeyCode.Escape;
        [SerializeField]
        private KeyCode leftBumper = KeyCode.R;
        [SerializeField]
        private KeyCode rightBumper = KeyCode.T;
        [SerializeField]
        private KeyCode leftTrigger = KeyCode.Mouse0;
        [SerializeField]
        private KeyCode rightTrigger = KeyCode.Mouse1;
        // Same signs are identical for right stick and arrows
        // LX - Left X || RX - Right X || UY - Up Y || DY - Down Y
        [SerializeField]
        private KeyCode leftStick_LX = KeyCode.A;
        [SerializeField]
        private KeyCode leftStick_RX = KeyCode.D;
        [SerializeField]
        private KeyCode leftStick_UY = KeyCode.W;
        [SerializeField]
        private KeyCode leftStick_DY = KeyCode.S;
        [SerializeField]
        private KeyCode rightStick_LX = KeyCode.LeftArrow;
        [SerializeField]
        private KeyCode rightStick_RX = KeyCode.RightArrow;
        [SerializeField]
        private KeyCode rightStick_UY = KeyCode.UpArrow;
        [SerializeField]
        private KeyCode rightStick_DY = KeyCode.DownArrow;
        [SerializeField]
        private KeyCode arrows_LX = KeyCode.A;
        [SerializeField]
        private KeyCode arrows_RX = KeyCode.D;
        [SerializeField]
        private KeyCode arrows_UY = KeyCode.W;
        [SerializeField]
        private KeyCode arrows_DY = KeyCode.S;

        public KeyCode GetKey(ButtonCode code)
        {
            switch (code) {
                case ButtonCode.A:
                    return A;
                case ButtonCode.B:
                    return B;
                case ButtonCode.X:
                    return X;
                case ButtonCode.Y:
                    return Y;
                case ButtonCode.LeftBumper:
                    return LeftBumper;
                case ButtonCode.RightBumper:
                    return RightBumper;
                case ButtonCode.Start:
                    return Start;
                case ButtonCode.Back:
                    return Back;
                case ButtonCode.LeftStick:
                    return LeftStick;
                case ButtonCode.RightStick:
                    return RightStick;
                default:
                    throw new System.Exception( "Wrong ButtonCode - KeyboardSettings::GetKey" );
            }
        }

        public KeyCode GetKey(AxisCode code)
        {
            switch (code) {
                case AxisCode.LeftTrigger:
                    return LeftTrigger;
                case AxisCode.RightTrigger:
                    return RightTrigger;
                default:
                    throw new System.Exception( "Wrong AxisCode - KeyboardSettings::GetKey" );
            }
        }

        public float GetKeyX(AxisCode code)
        {
            // In each case, we checks if button on the left or right is pressed, if user holds
            // two buttons in the same time, it results zero
            float result = 0;
            switch (code) {
                case AxisCode.LeftStick:
                    result += ( Input.GetKey( LeftStick_LX ) ? -1 : 0 ) + ( Input.GetKey( LeftStick_RX ) ? 1 : 0 );
                    break;
                case AxisCode.RightStick:
                    result += ( Input.GetKey( RightStick_LX ) ? -1 : 0 ) + ( Input.GetKey( RightStick_RX ) ? 1 : 0 );
                    break;
                case AxisCode.Arrows:
                    result += ( Input.GetKey( Arrows_LX ) ? -1 : 0 ) + ( Input.GetKey( Arrows_RX ) ? 1 : 0 );
                    break;
                default:
                    throw new System.Exception( "Wrong AxisCode - KeyboardSettings::GetKeyX" );
            }
            return result;
        }

        public float GetKeyY(AxisCode code)
        {
            // In each case, we checks if button on up or down is pressed, if user holds
            // two buttons in the same time, it results zero
            float result = 0;
            switch (code) {
                case AxisCode.LeftStick:
                    result += ( Input.GetKey( LeftStick_DY ) ? -1 : 0 ) + ( Input.GetKey( LeftStick_UY ) ? 1 : 0 );
                    break;
                case AxisCode.RightStick:
                    result += ( Input.GetKey( RightStick_DY ) ? -1 : 0 ) + ( Input.GetKey( RightStick_UY ) ? 1 : 0 );
                    break;
                case AxisCode.Arrows:
                    result += ( Input.GetKey( Arrows_DY ) ? -1 : 0 ) + ( Input.GetKey( Arrows_UY ) ? 1 : 0 );
                    break;
                default:
                    throw new System.Exception( "Wrong AxisCode - KeyboardSettings::GetKeyY" );
            }
            return result;
        }

        #region Getters
        public KeyCode A { get => a; }
        public KeyCode B { get => b; }
        public KeyCode X { get => x; }
        public KeyCode Y { get => y; }
        public KeyCode LeftStick { get => leftStick; }
        public KeyCode RightStick { get => rightStick; }
        public KeyCode Back { get => back; }
        public KeyCode Start { get => start; }
        public KeyCode LeftBumper { get => leftBumper; }
        public KeyCode RightBumper { get => rightBumper; }
        public KeyCode LeftTrigger { get => leftTrigger; }
        public KeyCode RightTrigger { get => rightTrigger; }
        public KeyCode LeftStick_LX { get => leftStick_LX; }
        public KeyCode LeftStick_RX { get => leftStick_RX; }
        public KeyCode LeftStick_UY { get => leftStick_UY; }
        public KeyCode LeftStick_DY { get => leftStick_DY; }
        public KeyCode RightStick_LX { get => rightStick_LX; }
        public KeyCode RightStick_RX { get => rightStick_RX; }
        public KeyCode RightStick_UY { get => rightStick_UY; }
        public KeyCode RightStick_DY { get => rightStick_DY; }
        public KeyCode Arrows_LX { get => arrows_LX; }
        public KeyCode Arrows_RX { get => arrows_RX; }
        public KeyCode Arrows_UY { get => arrows_UY; }
        public KeyCode Arrows_DY { get => arrows_DY; }
        #endregion
    }

}