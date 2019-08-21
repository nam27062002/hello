using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialDragonBarElement : MonoBehaviour {
    public enum State {
        LOCKED = 0,
        OWNED = 1
    }

    //---[Published Attributes]-------------------------------------------------
    [Separator("Lock")]
    [SerializeField] private GameObject m_lockedGroup;
    [SerializeField] private GameObject m_unlockedGroup;
    

    [Separator("State Control")]
    [SerializeField] protected State m_state;

    [Separator("Bar")]
    [SerializeField] private RectTransform m_scaleTransform = null;


    //---[Attributes]-----------------------------------------------------------
    protected int m_unlockLevel;

    private RectTransform __transform;
    private RectTransform m_transform {
        get {
            if (__transform == null) {
                __transform = GetComponent<RectTransform>();
            }
            return __transform;
        }
    }


    //---[Generic Methods]------------------------------------------------------
    private void Awake() {
        __transform = GetComponent<RectTransform>();

        m_state = State.LOCKED;
    }

    protected virtual void OnEnable() {
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

        switch(_state) {
            case State.LOCKED:
            OnLocked();
            break;

            case State.OWNED:
            OnOwned();
            break;
        }
    }

    public void SetUnlockLevel(int _level)
    {
        m_unlockLevel = _level;
    }


    private void OnLocked()   {

        m_lockedGroup.SetActive(true);
        if (m_unlockedGroup != null)
        {
            m_unlockedGroup.SetActive(false);
        }
    }

    private void OnOwned()    {

        m_lockedGroup.SetActive(false);
        if (m_unlockedGroup != null)
        {
            m_unlockedGroup.SetActive(true);
        }
    }

}
