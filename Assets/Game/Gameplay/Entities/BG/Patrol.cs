using UnityEngine;
using System.Collections;

public class Patrol : MonoBehaviour {

	private enum State {
		Walk = 0,
		Idle
	}

	[SerializeField] private Vector3[]  m_nodes = new Vector3[2];
	[SerializeField] private Color  	m_nodeColor = Colors.magenta;
	[SerializeField] private bool		m_circular = false;

	[SerializeField] private float m_speed = 3f;
	[SerializeField] private float m_rotationSpeed = 420f;
	[SerializeField][Range(0f, 1f)] private float m_idleChance = 0.5f;
	[SerializeField] private Range m_idleTimeAtDestination = new Range(2f, 5f);

	private Animator m_animator;

	private Transform m_transform;
	private Vector3 m_startPosition;
	private Vector3 m_position;
	private Vector3 m_direction;

	private Quaternion m_rotation;
	private Quaternion m_targetRotation;

	private int m_targetIndex;
	private int m_listDirection;

	private float m_timer;

	private State m_state;



	//---------------------------------------------------------------------------------
	// Use this for initialization
	void Start () {
		m_animator = GetComponent<Animator>();

		m_transform = transform;
		m_startPosition = m_transform.position;
		if (m_nodes.Length > 0) {
			m_transform.position = GetNodePosition(0);
		}
		m_position = m_transform.position;

		m_targetIndex = 0;
		m_listDirection = 1;

		m_rotation = transform.rotation;

		Idle();
	}
	
	// Update is called once per frame
	void Update () {
		switch(m_state) {
			case State.Idle: {
					if (m_nodes.Length > 1) {
						m_timer -= Time.deltaTime;
						if (m_timer <= 0f) {
							Walk();
						}
					}
				} break;

			case State.Walk: {
					float delta = Time.deltaTime * m_speed;
					float dSqr = (GetNodePosition(m_targetIndex) - m_position).sqrMagnitude;

					if (dSqr > delta * delta) {
						m_position += m_direction * delta;
					} else {
						m_position = GetNodePosition(m_targetIndex);
						if (Random.Range(0f, 1f) < m_idleChance) {
							Idle();
						} else {
							Walk();
						}
					}

					m_transform.position = m_position;
				} break;
		}

		// face to the correct direction
		m_rotation = Quaternion.RotateTowards(m_rotation, m_targetRotation, Time.deltaTime * m_rotationSpeed);
		m_transform.rotation = m_rotation;
	}

	void Idle() {
		m_animator.SetBool( GameConstants.Animator.MOVE , false);
		m_timer = m_idleTimeAtDestination.GetRandom();

		// direction
		m_direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, -1f));
		m_targetRotation = Quaternion.LookRotation(m_direction + Vector3.back * 0.1f, Vector3.up);

		m_state = State.Idle;
	}

	void Walk() {
		m_animator.SetBool( GameConstants.Animator.MOVE, true);

		//change target
		if (m_circular) {
			m_targetIndex = (m_targetIndex + 1) % m_nodes.Length;
		} else {
			if (m_targetIndex == m_nodes.Length - 1) {
				m_listDirection = -1;
			}

			if (m_targetIndex == 0) {
				m_listDirection = 1;
			}

			m_targetIndex += m_listDirection;
		}

		m_direction = GetNodePosition(m_targetIndex) - m_position;
		m_direction.Normalize();

		Vector3 lookAt = m_direction;
		lookAt.y = 0;
		m_targetRotation = Quaternion.LookRotation(lookAt + Vector3.back * 0.1f, Vector3.up);		

		m_state = State.Walk;
	}

	private Vector3 GetNodePosition(int _index) {
		return m_nodes[_index] + m_startPosition;
	}

	void OnDrawGizmosSelected() {
		for (int i = 0; i < m_nodes.Length; i++) {
			Gizmos.color = m_nodeColor;
			Gizmos.DrawCube(m_nodes[i] + transform.position + Vector3.up * 1f, new Vector3(0.25f, 2f, 0.25f));
		}
	}
}
