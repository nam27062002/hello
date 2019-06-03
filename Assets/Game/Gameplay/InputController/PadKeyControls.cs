using UnityEngine;
using InControl;


public class PadKeyControls : MonoBehaviour {
    public class InputModuleActions : PlayerActionSet {
        public PlayerAction Boost;
        public PlayerAction Left;
        public PlayerAction Right;
        public PlayerAction Up;
        public PlayerAction Down;
        public PlayerTwoAxisAction Move;


        public InputModuleActions() {
            Boost = CreatePlayerAction("Boost");
            Left = CreatePlayerAction("Move Left");
            Right = CreatePlayerAction("Move Right");
            Up = CreatePlayerAction("Move Up");
            Down = CreatePlayerAction("Move Down");
            Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
        }
    }

    [SerializeField] private InControlInputModule m_inputModule;
    private InputModuleActions m_actions;
    private Vector3 m_direction = GameConstants.Vector3.zero;
    public Vector3 direction { get { return m_direction; } }


    void OnEnable() {
        m_actions = new InputModuleActions();

        m_actions.Boost.AddDefaultBinding(InputControlType.Action1);
        m_actions.Boost.AddDefaultBinding(Key.X);

        m_actions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
        m_actions.Up.AddDefaultBinding(Key.UpArrow);

        m_actions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
        m_actions.Down.AddDefaultBinding(Key.DownArrow);

        m_actions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
        m_actions.Left.AddDefaultBinding(Key.LeftArrow);

        m_actions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
        m_actions.Right.AddDefaultBinding(Key.RightArrow);


        if (m_inputModule != null) {
            m_inputModule.SubmitAction = m_actions.Boost;
            m_inputModule.MoveAction = m_actions.Move;
        }
    }

    void OnDisable() {
        m_actions.Destroy();
    }

    public void UpdateJoystickControls() {
        if (m_actions.Move.IsPressed) {
            m_direction.x = m_actions.Move.Value.x;
            m_direction.y = m_actions.Move.Value.y;
            m_direction.z = 0f;
        }
    }

    public bool isMoving()  { return m_actions.Move.IsPressed; }
    public bool getAction() { return m_actions.Boost.IsPressed; }
}
