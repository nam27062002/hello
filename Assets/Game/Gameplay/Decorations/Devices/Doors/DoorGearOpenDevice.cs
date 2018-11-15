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
    [SerializeField] private string m_onActiveAudio = null;
    private AudioObject m_onActiveAudioAO;

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

    private void OnDestroy() {
        if (m_onActiveAudioAO != null && m_onActiveAudioAO.IsPlaying()) {
            m_onActiveAudioAO.Stop();
        }
        RemoveAudioParent(ref m_onActiveAudioAO);
    }

    private void RemoveAudioParent(ref AudioObject ao) {
        if (ao != null && ao.transform.parent == transform) {
            ao.transform.parent = null;
            ao.completelyPlayedDelegate = null;
            if (ao.IsPlaying() && ao.audioItem.Loop != AudioItem.LoopMode.DoNotLoop)
                ao.Stop();
        }
        ao = null;
    }

    private void OnTriggerBreak() {
        if (m_state == State.IDLE) {
            for (int i = 0; i < m_gears.Length; ++i) {
                m_rotations[i] = 0f;
                m_initialRotations[i] = m_gears[i].localRotation.eulerAngles;
            }
            m_door.Open();

            if (!string.IsNullOrEmpty(m_onActiveAudio)) {
                m_onActiveAudioAO = AudioController.Play(m_onActiveAudio, transform);
            }

            m_state = State.ACTIVE;
        }
    }

    private void Update() {
        if (m_state == State.ACTIVE) {
            if (m_door.isOpen()) {
                RemoveAudioParent(ref m_onActiveAudioAO);
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