using UnityEngine;
using System.Collections.Generic;

public class FireNode : MonoBehaviour {

	enum State {
		Idle,
		Damaged,
		Burning,
		Burned
	};

	[SerializeField] private float m_resistance;
	[SerializeField] private float m_burningTime;
	[SerializeField] private float m_damage;
	[SerializeField] private float m_checkFireTime = 0.25f;


	private DragonBreathBehaviour m_breath;
	private List<FireNode> m_neighbours;
	private State m_state;
	
	private float m_timer;



	// Use this for initialization
	void Start () {
	
		// get player breath component
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
		m_timer = m_checkFireTime;

		// get two closets neighbours
		m_neighbours = new List<FireNode>();
		FireNode[] nodes = transform.parent.GetComponentsInChildren<FireNode>();

		int numNeighbours = 2; //nearest nodes
		for (int n = 0; n < numNeighbours; n++) {
			float minD = 0;
			int index = -1;
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes[i] != null && nodes[i] != this) {
					float d = (nodes[i].transform.position - transform.position).sqrMagnitude;

					if (index == -1 || d < minD) {
						index = i;
						minD = d;
					}
				}
			}

			if (index >= 0) {
				m_neighbours.Add(nodes[index]);
				nodes[index] = null;
			}
		}

		m_state = State.Idle;
	}

	void Update() {

		if (m_state == State.Idle || m_state == State.Damaged) {
			
			//check if this intersecs with dragon breath
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_timer = m_checkFireTime;
				if (m_breath.IsInsideArea(transform.position)) {
					Burn(m_breath.damage);
				}
			}
		} else if (m_state == State.Burning) {
			
			//burn near nodes and fuel them
			m_timer -= Time.deltaTime;
			if (m_timer > 0) {
				for (int i = 0; i < m_neighbours.Count; i++) {
					m_neighbours[i].Burn(m_damage); // what amount of damage should
				}
			} else {
				m_state = State.Burned;
			}
		}
	}

	public void Burn(float _damage) {

		if (m_state == State.Idle || m_state == State.Damaged) {
			m_resistance -= _damage;
			m_state = State.Damaged;

			if (m_resistance <= 0) {
				m_state = State.Burning;
				m_timer = m_burningTime;
			}
		}
	}

	void OnDrawGizmos() {

		Gizmos.color = new Color(0.69f, 0.09f, 0.12f);

		if (m_state == State.Damaged) {
			Gizmos.color = Color.yellow;
		} else if (m_state == State.Burning) {
			Gizmos.color = Color.magenta;
		} else if (m_state == State.Burned) {
			Gizmos.color = Color.black;
		}

		Gizmos.DrawSphere(transform.position, 0.5f);

		if (m_neighbours != null) {
			for (int i = 0; i < m_neighbours.Count; i++) {
				if (m_state != State.Burning) {
					Color color = Gizmos.color;
					color.a = 0.2f;
					Gizmos.color = color;
				}

				Gizmos.DrawLine(transform.position, m_neighbours[i].transform.position);
			}
		}
	}
}
