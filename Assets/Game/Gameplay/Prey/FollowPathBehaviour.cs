using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyMotion))]
public class FollowPathBehaviour : Initializable {

	private PathController m_path;
	public PathController path { set { m_path = value; } }

	private PreyMotion m_motion;

	private Vector3 m_target;

	// Use this for initialization
	void Awake () {		
		m_motion = GetComponent<PreyMotion>();
	}

	public override void Initialize() {			
		if (m_path != null) {
			m_target = m_path.GetNext();			
		}
	}

	void OnEnable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}
	}

	public void SetPath(PathController _path) {
		m_path = _path;
		m_target = m_path.GetNext();
	}
	
	// Update is called once per frame
	void Update () {
		if (m_path != null) {
			if (Vector2.Distance(m_motion.position, m_target) <= m_path.radius) {
				m_target = m_path.GetNext();
			}

			m_motion.Seek(m_target);
		}

		m_motion.ApplySteering();
	}
}
