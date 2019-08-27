using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewParticleSpawner : MonoBehaviour, IBroadcastListener {
    private enum ActivationMode {
        AUTO,
        MANUAL
    }

    private enum State {
        IDLE = 0,
        SPAWN_WHEN_VISIBLE,
        SPAWNED,
        RETURN_NOT_VISIBLE,
        STOP
    }

    [SerializeField] private ActivationMode m_activationMode = ActivationMode.AUTO;
    [Comment("Where the particles will be spawned.\nLeave empty to use this transform as parent.")]
    [SerializeField] private Transform m_parent;
    [Comment("View's bounds will be compared to the camera frustum to trigger the particles.\nLeave empty to use parent's position instead.")]
    [SerializeField] private Renderer m_view;
    [Space]
    [SerializeField] private ParticleData[] m_particleDatas;

    private GameCamera m_camera;

    private GameObject[] m_particleSytems;
    private ParticleControl[] m_particleControl;

    private State m_state;


    // Use this for initialization
    void Awake() {
        m_camera = null;
        if (Camera.main != null) {
            m_camera = Camera.main.GetComponent<GameCamera>();
        }

        if (m_parent == null) {
            m_parent = transform;
        }

        m_particleSytems = new GameObject[m_particleDatas.Length];
        m_particleControl = new ParticleControl[m_particleDatas.Length];

        for (int i = 0; i < m_particleDatas.Length; ++i) {
            m_particleDatas[i].CreatePool();
        }

        m_state = State.IDLE;
    }

    void Start() {
        Messenger.AddListener<string>(MessengerEvents.SCENE_PREUNLOAD, OnScenePreunload);
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

    void OnDestroy() {
        Messenger.RemoveListener<string>(MessengerEvents.SCENE_PREUNLOAD, OnScenePreunload);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

    void OnScenePreunload(string _scene) {
        enabled = false;
    }
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER:
            for (int i = 0; i < m_particleDatas.Length; ++i) {
                m_particleDatas[i].CreatePool();
            }
            break;
        }
    }

    void OnDisable() {
        ForceReturn();
    }

    void Update() {
        // Show / Hide effect if this node is inside Camera or not
        bool isInsideActivationMaxArea = CheckActivationArea();

        switch (m_state) {
            case State.IDLE: {
                    if (isInsideActivationMaxArea) {
                        // If activation mode is AUTO, spawn!
                        if (m_activationMode == ActivationMode.AUTO) {
                            SpawnInternal();
                        }
                    }
                }
                break;

            case State.SPAWN_WHEN_VISIBLE: {
                    if (isInsideActivationMaxArea) {
                        SpawnInternal();
                    }
                }
                break;

            case State.SPAWNED: {
                    if (!isInsideActivationMaxArea) {
                        m_state = State.RETURN_NOT_VISIBLE;
                    }
                }
                break;

            case State.RETURN_NOT_VISIBLE: {
                    if (isInsideActivationMaxArea && m_activationMode == ActivationMode.AUTO) {
                        CancelReturn();
                    } else {
                        if (StopAndReturn()) {
                            m_state = State.SPAWN_WHEN_VISIBLE;
                        }
                    }
                }
                break;

            case State.STOP: {
                    if (StopAndReturn()) {
                        m_state = State.IDLE;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Check whether the particle can be spawned and do it.
    /// Only for <c>ActivationMode.MANUAL</c>.
    /// </summary>
    public void Spawn() {
        // Only for manual activation mode
        if (m_activationMode != ActivationMode.MANUAL) return;

        // Are we inside the activation area?
        if (!CheckActivationArea()) {
            m_state = State.SPAWN_WHEN_VISIBLE;
            return;
        }

        // Only if not already spawned
        if (m_state == State.IDLE) {
            // Everything ok! Spawn the particle
            SpawnInternal();
        } else {
            CancelReturn();
        }
    }

    /// <summary>
    /// Force the active particle to stop and returns it to the pool.
    /// Only for <c>ActivationMode.MANUAL</c>.
    /// </summary>
    public void Stop() {
        // Only for manual activation mode
        if (m_activationMode != ActivationMode.MANUAL) return;

        if (m_state == State.SPAWN_WHEN_VISIBLE) {
            m_state = State.IDLE;
            return;
        }

        // Only if actually spawned
        if (m_state != State.SPAWNED) return;

        // Everything ok! Stop the particle
        m_state = State.STOP;
    }

    /// <summary>
    /// Check whether the spawner is inside the activation area and thus can be spawned.
    /// </summary>
    /// <returns>Whether the spawner is within the activation area.</returns>
    protected bool CheckActivationArea() {
        bool isInsideActivationMaxArea = false;

        if (m_camera != null) {
            if (m_view != null) {
                isInsideActivationMaxArea = m_camera.IsInsideCameraFrustrum(m_view.bounds);
            } else {
                isInsideActivationMaxArea = m_camera.IsInsideCameraFrustrum(m_parent.position);
            }
        } else {
            // [AOC] Probably not in-game, always show the effect (i.e. Menu)
            isInsideActivationMaxArea = true;
        }

        return isInsideActivationMaxArea;
    }

    protected virtual void SpawnInternal() {
        for (int i = 0; i < m_particleDatas.Length; ++i) {
            m_particleSytems[i] = m_particleDatas[i].Spawn(m_parent, m_particleDatas[i].offset, true);
            if (m_particleSytems[i] != null) {
                m_particleControl[i] = m_particleSytems[i].GetComponent<ParticleControl>();
            }
        }

        m_state = State.SPAWNED;
    }

    protected virtual void CancelReturn() {
        for (int i = 0; i < m_particleControl.Length; ++i) {
            if (m_particleControl[i] != null) {
                m_particleControl[i].Play(m_particleDatas[i]);
            }
        }
        m_state = State.SPAWNED;
    }

    protected virtual bool StopAndReturn() {
        bool areStopped = true;
        for (int i = 0; i < m_particleControl.Length; ++i) {
            if (m_particleControl[i] != null) {
                areStopped = areStopped && m_particleControl[i].Stop();
            }
        }

        if (areStopped) {
            ForceReturn();
        }

        return areStopped;
    }

    protected virtual void ForceReturn() {
        for (int i = 0; i < m_particleSytems.Length; ++i) {
            if (m_particleSytems[i] != null) {
                m_particleDatas[i].ReturnInstance(m_particleSytems[i]);
            }
            m_particleSytems[i] = null;
            m_particleControl[i] = null;
        }
    }
}
