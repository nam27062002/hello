using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabDragonBarElement : MonoBehaviour {
    public enum State {
        LOCKED = 0,
        AVAILABLE,
        OWNED
    }


    //---[Published Attributes]-------------------------------------------------
    [Separator("State Control")]
    [SerializeField] private Animator m_animator = null;

    [Separator("Bar")]
    [SerializeField] private RectTransform m_scaleTransform = null;


    //---[Attributes]-----------------------------------------------------------
    private RectTransform __transform;
    private RectTransform m_transform {
        get {
            if (__transform == null) {
                __transform = GetComponent<RectTransform>();
            }
            return __transform;
        }
    }

    protected State m_state;


    //---[Generic Methods]------------------------------------------------------
    private void Awake() {
        __transform = GetComponent<RectTransform>();

        m_state = State.LOCKED;
    }

    protected virtual void OnEnable() {
        m_animator.SetInteger(GameConstants.Animator.STATE, (int)m_state);
    }

	protected virtual void OnDisable() {

	}

    //---[Queries]--------------------------------------------------------------
    public void SetPos(float _x, float _y) {
        m_transform.localPosition = new Vector3(_x, _y, 0f);
    }

    public void SetGlobalScale(float _sx, float _sy) {
        m_transform.localScale = new Vector2(_sx, _sy);
    }

    public void SetLocalScale(float _sx, float _sy) {
        m_scaleTransform.localScale = new Vector2(_sx, _sy);
    }

    public float GetWidth() {
        return m_scaleTransform.sizeDelta.x;
    }

    public float GetHeight() {
        return m_scaleTransform.sizeDelta.y;
    }


    //---[Public Methods]-------------------------------------------------------
    public void SetState(State _state) {
        m_state = _state;

        m_animator.SetInteger(GameConstants.Animator.STATE, (int)m_state);

        switch(_state) {
            case State.LOCKED:
            OnLocked();
            break;

            case State.AVAILABLE:
            OnAvailable();
            break;

            case State.OWNED:
            OnOwned();
            break;
        }
    }


    //---[Abstract/Virtual Methods]---------------------------------------------
    protected virtual void OnLocked()   {}
    protected virtual void OnAvailable(){}
    protected virtual void OnOwned()    {}


    //---[Abstract/Virtual Methods]---------------------------------------------
    private void OnDrawGizmosSelected() {
        if (m_animator != null) {
            m_animator.Update(1f);
        }
    }
}
