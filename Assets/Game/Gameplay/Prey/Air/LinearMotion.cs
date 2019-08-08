using UnityEngine;
using System.Collections;

public class LinearMotion : ISpawnable {

    [SerializeField] private Vector3 m_direction = GameConstants.Vector3.right;
	[SerializeField] private float m_speed = 1;

	private float m_time;
    private Transform m_transform;
	private Vector3 m_originalPostion;
	public Vector3 originalPostion { get { return m_originalPostion; } }

	void Start() {
        m_transform = transform;
        m_originalPostion = m_transform.position;
	}

	override public void Spawn(ISpawner _spawner) {
		m_time = 0f;
		m_originalPostion = m_transform.position;
	}

	override public void CustomUpdate() {}


	// Update is called once per frame
	void Update() {	
		if (m_speed > 0) {
			Vector3 position = m_transform.position;
            position = m_originalPostion + m_direction * m_speed * m_time;
            m_transform.position = position;
			m_time += Time.deltaTime;
		}
	}

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + m_direction * 10f);

        transform.rotation = Quaternion.LookRotation(m_direction, GameConstants.Vector3.up);
    }
}
