using UnityEngine;

public class DisableInSeconds : MonoBehaviour {

	public enum PoolType {
		PoolManager = 0,
		ParticleManager
	};

	[SerializeField] private float m_activeTime = 1f;
	[SerializeField] private PoolType m_returnTo = PoolType.PoolManager;

	private float m_activeTimer;

	void OnEnable() {
		m_activeTimer = m_activeTime;
	}

	void Update() {		
		m_activeTimer -= Time.deltaTime;
		if (m_activeTimer < 0f) {
			gameObject.SetActive(false);
			switch(m_returnTo) {
				case PoolType.PoolManager: 		PoolManager.ReturnInstance(gameObject); 	break;
				case PoolType.ParticleManager: 	ParticleManager.ReturnInstance(gameObject); break;
			}
		}
	}
}
