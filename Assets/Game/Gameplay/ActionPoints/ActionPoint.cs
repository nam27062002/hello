using UnityEngine;
using System.Collections;

public class ActionPoint : MonoBehaviour, IQuadTreeItem {

	[SerializeField] private float m_radius;

	private Rect m_boundingRect;
	public Rect boundingRect { get { return m_boundingRect; } }

	void Awake() {
		m_boundingRect = new Rect(transform.position - Vector3.one * m_radius, Vector2.one * m_radius);
	}

	void Start() {
		ActionPointManager.instance.Register(this);
	}

	void OnDrawGizmos() {
		Gizmos.color = Colors.coral;
		Gizmos.DrawWireSphere(transform.position, m_radius);

		Gizmos.color = Colors.red;
		Gizmos.DrawCube(transform.position + Vector3.up * (1f + m_radius), new Vector3(1f, 0.4f, 0.4f));
		Gizmos.DrawCube(transform.position + Vector3.up * (1f + m_radius), new Vector3(0.4f, 1f, 0.4f));
	}
}
