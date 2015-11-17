using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyMotion))]
public class FollowPathBehaviour : Initializable {

	private PathController m_path;
	public PathController path { set { m_path = value; } }

	private PreyMotion m_motion;
	private Animator m_animator;

	private Vector3 m_target;

	// Use this for initialization
	void Awake () {		
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}

	public override void Initialize() {			
		if (m_path != null) {
			m_target = m_path.GetNext();			
		}
		m_animator.SetBool("move", true);
	}

	void OnEnable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}				
		m_animator.SetBool("move", true);
	}

	void OnDisable() {
		if (m_path != null) {
			m_target = m_path.GetNearestTo(m_motion.position);
		}		
		m_animator.SetBool("move", false);
	}

	public void SetPath(PathController _path) {
		m_path = _path;
		m_target = m_path.GetNext();
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		if (m_path != null) {
			if (Vector2.Distance(m_motion.position, m_target) <= m_path.radius) {
				m_target = m_path.GetNext();
			}

			m_motion.Seek(m_target);
		}

		m_motion.ApplySteering();
	}
}
