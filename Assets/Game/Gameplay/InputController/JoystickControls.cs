using UnityEngine;
using InControl;


public class JoystickControls : MonoBehaviour {
    protected Vector3 m_direction = GameConstants.Vector3.zero;
    public Vector3 direction { get { return m_direction; } }

    bool stickPressed = false;
    bool actionPressed = false;

    public void UpdateJoystickControls() {
        InputDevice device = InputManager.ActiveDevice;

        if (device != null && device.IsActive && device.DeviceClass == InputDeviceClass.Controller) {
            TwoAxisInputControl leftStick = device.LeftStick;
            m_direction.x = leftStick.X;
            m_direction.y = leftStick.Y;
            m_direction.z = 0f;
            stickPressed = m_direction.sqrMagnitude > Mathf.Epsilon;
            actionPressed = device.Action1.IsPressed;
        } else {
            stickPressed = false;
            actionPressed = false;
        }
    }


    public bool isMoving()  { return stickPressed; }
    public bool getAction() { return actionPressed; }
}
