using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
    private enum State {
        IDLE = 0,
        OPENING,
        OPEN
    }

    [SerializeField] private Transform m_view = null;
    [SerializeField] private float m_time = 2f;
    [SerializeField] private float m_openDistance = 0f;
    [SerializeField] private float m_scaleYatTop = 1f;
	

    private State m_state;
    private float m_timer;


    private void Awake() {
        m_state = State.IDLE;
    }

    private void Update() {
        if (m_state == State.OPENING) {
            m_timer += Time.deltaTime;

            Vector3 localPos = m_view.localPosition;
            localPos.y = Mathf.Lerp(0, m_openDistance, m_timer / m_time);
            m_view.localPosition = localPos;

            if (m_timer >= m_time) {
                m_state = State.OPEN;
                m_view.localScale = new Vector3(1f, m_scaleYatTop, 1f);
            }
        }
    }

    public void Open() {
        if (m_state == State.IDLE) {
            m_timer = 0f;
            m_state = State.OPENING;
        }
    }

    public bool isOpen() {
        return m_state == State.OPEN;
    }

}
