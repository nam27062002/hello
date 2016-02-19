using UnityEngine;

public class DisableInSeconds : MonoBehaviour {

	[SerializeField] private float m_activeTime = 1f;
	private float m_activeTimer;

	void OnEnable() {
		m_activeTimer = m_activeTime;
	}

	void Update() {		
		m_activeTimer -= Time.deltaTime;
		if (m_activeTimer < 0f) {
			gameObject.SetActive(false);
			PoolManager.ReturnInstance( gameObject );
		}
	}
}
