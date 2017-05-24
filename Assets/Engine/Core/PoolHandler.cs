using UnityEngine;

public class PoolHandler {

	private Pool m_pool;
	private bool m_isValid;

	public PoolHandler(Pool _pool) {
		m_pool = _pool;
		m_isValid = true;
	}

	public void Invalidate() { m_isValid = false; }

	public GameObject GetInstance(bool _activate = true) {
		if (m_isValid) {
			return m_pool.Get(_activate);
		}
		Debug.LogError("[Pool] invalid pool handler");
		return null;
	}

	public void ReturnInstance(GameObject _go) {
		if (m_isValid) {
			m_pool.Return(_go);
		} else {
			Debug.LogError("[Pool] invalid pool handler");
		}
	}
}
