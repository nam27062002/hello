using UnityEngine;


public class DoorGearOpenDevice : MonoBehaviour {
    private enum State {
        IDLE = 0,
        ACTIVE,
        DISABLED
    }


    [SerializeField] private Door m_door = null;
    [SerializeField] private Transform[] m_gears = null;
    [SerializeField] private float[] m_gearsRotationSpeed = null;
    [SerializeField] private Vector3 m_rotationAxis = GameConstants.Vector3.forward;
    [SerializeField] private BreakableBehaviour m_breakableTrigger = null;
    [SerializeField] private InflammableDecoration m_inflammableTrigger = null;
       

    private State m_state;
    private float[] m_rotations;
    private Vector3[] m_initialRotations;


    private void Awake() {
        m_state = State.IDLE;
        m_rotations = new float[m_gears.Length];
        m_initialRotations = new Vector3[m_gears.Length];
        m_breakableTrigger.onBreak += OnTriggerBreak;
        m_inflammableTrigger.onBurn += OnTriggerBreak;
    }

    private void OnTriggerBreak() {
        if (m_state == State.IDLE) {
            for (int i = 0; i < m_gears.Length; ++i) {
                m_rotations[i] = 0f;
                m_initialRotations[i] = m_gears[i].localRotation.eulerAngles;
            }
            m_door.Open();
            m_state = State.ACTIVE;
        }
    }

    private void Update() {
        if (m_state == State.ACTIVE) {
            if (m_door.isOpen()) {
                m_state = State.DISABLED;
            } else {
                Rotate(Time.deltaTime);
            }
        }
    }

    private void Rotate(float _dt) {
        for (int i = 0; i < m_gears.Length; ++i) {
            m_rotations[i] += m_gearsRotationSpeed[i] * _dt;
            m_gears[i].localRotation = Quaternion.Euler(m_initialRotations[i]) *  Quaternion.AngleAxis(m_rotations[i], (1 - (2 * (i % 2))) * m_rotationAxis);
        }
    }

    public void DebugAnimation(float _dt) {
        if (m_rotations == null || m_rotations.Length < m_gears.Length) {
            m_rotations = new float[m_gears.Length];
            m_initialRotations = new Vector3[m_gears.Length];
        }

        for (int i = 0; i < m_gears.Length; ++i) {
            m_rotations[i] = 0f;
            m_initialRotations[i] = m_gears[i].localRotation.eulerAngles;
        }

        Rotate(_dt);
    }
}