using UnityEngine;

public class DestroyInSeconds : MonoBehaviour {

	[SerializeField] private float m_lifeTime = 1f;
		
	void Update() {		
		m_lifeTime -= Time.deltaTime;
		if (m_lifeTime < 0f)
			DestroyObject(gameObject);
	}
}
