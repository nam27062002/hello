using UnityEngine;

public class DestroyInSeconds : MonoBehaviour {

	[SerializeField] private float m_lifeTime = 1f;
	public float lifeTime {
		get { return m_lifeTime; }
		set { m_lifeTime = value; }
	}
		
    void Awake() {
        // If it has to be destroyed immediately (typically because this game object is a placeholder object used in edit mode)
        // then it's done as soon as possible in order to prevent other objects retrieving components from
        // getting components in this game object.
        if (m_lifeTime <= 0f) {
			DestroyObject(gameObject);
        }
    }

	void Update() {
        if (m_lifeTime > 0f) {
            m_lifeTime -= Time.deltaTime;
            if (m_lifeTime < 0f)
                DestroyObject(gameObject);
        }
	}
}
