using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialDragonBarElement : MonoBehaviour {
    public enum State {
        LOCKED = 0,
        OWNED = 1
    }

    //---[Published Attributes]-------------------------------------------------
	[InfoBox("The rect transform (size and pivot) of the prefab will be used as reference to distribute and scale the elements through the bar."+
		"\nDefine it accordingly, making the visuals of the different elements be properly aligned and spatiated." +
		"\nThe recommended setup is pivot at the center, size representing the space you expect the element to occupy in the bar."
	)]

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
            InitTransform();
            return __transform;
        }
    }

	private Vector2 m_size = GameConstants.Vector2.one;


    //---[Generic Methods]------------------------------------------------------
    private void Awake() {
		InitTransform();

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

	public Vector2 GetSize() {
		InitTransform();
		return m_size;
	}

    public float GetWidth() {
		return GetSize().x;
	}

    public float GetHeight() {
		return GetSize().y;
    }

    public Vector2 GetOffset()
    {
        return m_scaleTransform.anchoredPosition;
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

	//---[Internal Methods]-------------------------------------------------------
	private void InitTransform() {
		if(__transform != null) return;	// No need if already done

		__transform = GetComponent<RectTransform>();

		// Cache size
		// To properly figure out the size, reset anchors
		Vector2 minAnchorBackup = __transform.anchorMin;
		Vector2 maxAnchorBackup = __transform.anchorMax;

		__transform.anchorMin = GameConstants.Vector2.center;
		__transform.anchorMax = GameConstants.Vector2.center;

		m_size = __transform.sizeDelta;

		__transform.anchorMin = minAnchorBackup;
		__transform.anchorMax = maxAnchorBackup;
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
