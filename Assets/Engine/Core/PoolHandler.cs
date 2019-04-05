using UnityEngine;

public class PoolHandler {	
	private Pool m_pool;
	public Pool pool {
		get { return m_pool; }
	}

	private bool m_isValid;


	//-------------------------------------------------//

	public PoolHandler() {
		Invalidate();
	}

	public PoolHandler(Pool _pool) {
		AssignPool(_pool);
	}

	public void AssignPool(Pool _pool)	{ 
		if (_pool != null) {
			m_isValid = true;  
			m_pool = _pool;
		} else {
			Invalidate();
		}
	}

	public void Invalidate() {
		m_isValid = false;
		m_pool = null;  
	}

	public GameObject GetInstance(bool _activate = true) {
		if (m_isValid) {
			return m_pool.Get(_activate);
		}
#if DEBUG
        Debug.LogError("[Pool] invalid pool handler");
#endif
        return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_isValid) {
			m_pool.Return(_go);
		}
#if DEBUG
        else {
			Debug.LogError("[Pool] invalid pool handler");
        }
#endif
    }

	public bool HasAvailableInstances()	{
		if ( m_isValid )
		{
			return m_pool.canGrow || m_pool.NumFreeObjects() > 0;
		}
		return false;
	}

}
