using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CapsuleCollider))]
public class CaveEntrance : MonoBehaviour {

	[SerializeField] private Color m_ambientColor = Color.black;

	private Collider m_collider;
	private float m_distance;

	private Vector3 m_entryNode;
	private Vector3 m_exitNode;

	private Color m_colorLerp = Color.white;

	private Vector3 m_projection;

	private DragonTint m_dragonTint;

	// Use this for initialization
	void Start () {
		m_collider = GetComponent<Collider>();
		m_distance = m_collider.bounds.size.y;

		m_entryNode = transform.position + transform.rotation * (Vector3.up * m_distance * 0.5f);
		m_exitNode = transform.position + transform.rotation * (Vector3.down * m_distance * 0.5f);
		m_projection = m_entryNode;

		m_colorLerp = Color.white;

		//
		m_dragonTint = InstanceManager.player.GetComponent<DragonTint>();
	}

	void OnTriggerEnter(Collider _other) {
		float entryDist = (_other.transform.position - m_entryNode).sqrMagnitude;
		float exitDist = (_other.transform.position - m_exitNode).sqrMagnitude;

		if (entryDist < exitDist) {
			// go in
			m_colorLerp = Color.white;
			m_projection = m_entryNode;
		} else {
			// go out
			m_colorLerp = m_ambientColor;
			m_projection = m_exitNode;
		}
		m_dragonTint.SetCaveColor(m_colorLerp);
	}

	void OnTriggerStay(Collider _other) {
		Vector3 entryExit = m_exitNode - m_entryNode;
		m_projection = Vector3.Project(_other.transform.position - m_entryNode, entryExit);

		Debug.DrawLine(m_entryNode, _other.transform.position, Color.red);
		Debug.DrawLine(_other.transform.position, m_entryNode + m_projection, Color.cyan);

		m_colorLerp = Color.Lerp(Color.white, m_ambientColor, m_projection.sqrMagnitude / entryExit.sqrMagnitude);
		m_dragonTint.SetCaveColor(m_colorLerp);
	}

	void OnTriggerExit(Collider _other) {
		float entryDist = (_other.transform.position - m_entryNode).sqrMagnitude;
		float exitDist = (_other.transform.position - m_exitNode).sqrMagnitude;

		if (entryDist < exitDist) {
			// go in
			m_colorLerp = Color.white;
		} else {
			// go out
			m_colorLerp = m_ambientColor;
		}
		m_dragonTint.SetCaveColor(m_colorLerp);
	}

	void OnDrawGizmos() {
		if (m_collider == null) {
			m_collider = GetComponent<Collider>();
		} else {
			m_distance = m_collider.bounds.size.y;

			Vector3 pointA = transform.position + transform.rotation * (Vector3.up * m_distance * 0.5f);
			Vector3 pointB = transform.position + transform.rotation * (Vector3.down * m_distance * 0.5f);
			Vector3 dir = (pointB - pointA).normalized;

			Gizmos.color = Color.white;
			Gizmos.DrawLine(pointA, pointA + dir * m_distance * 0.5f); 
			Gizmos.DrawSphere(pointA, 2f);

			Gizmos.color = m_ambientColor;
			Gizmos.DrawLine(pointA + dir * m_distance * 0.5f, pointB);
			Gizmos.DrawSphere(pointB, 2f);

			Gizmos.color = m_colorLerp;
			Gizmos.DrawSphere(pointA + m_projection, 1.5f);
		}
	}
}
