using UnityEngine;
using System.Collections.Generic;

public class ActionPoint : MonoBehaviour, IQuadTreeItem {
	
	[SerializeField] private float m_radius;
	[SerializeField] private int m_capacity = 1;
	[SerializeField] private Actions m_actions;


	private Rect m_boundingRect;
	public Rect boundingRect { get { return m_boundingRect; } }

	private int m_members = 0;

    private bool m_hasToDoStart = true;

#if UNITY_EDITOR
    void Start() {
        if (m_hasToDoStart) {
            DoStart();
        }
    }
#endif

    public void DoStart() {
        if (m_hasToDoStart) {
            m_hasToDoStart = false;

            ActionPointManager.instance.Register(this);
            m_boundingRect = new Rect(transform.position - Vector3.one * m_radius, Vector2.one * m_radius * 2f);
            m_members = 0;
        }
    }
    
	public bool CanEnter() 	{ return m_members < m_capacity; }
	public void Enter()		{ m_members++; }
	public void Leave() 	{ m_members--; }

	public Actions.Action GetAction(ref Actions _entityActions) { return m_actions.GetAction(ref _entityActions); }
	public Actions.Action GetDefaultAction() { return m_actions.GetDefaultAction(); }

	//----------------------------------------------------------------------------------------------------------------------------//
	void OnDrawGizmos() {
		Gizmos.color = Colors.coral;
		Gizmos.DrawWireSphere(transform.position, m_radius);

		Gizmos.color = Colors.red;
		Gizmos.DrawWireCube(transform.position + Vector3.up * (1f + m_radius), new Vector3(1f, 0.4f, 0.4f));
		Gizmos.DrawWireCube(transform.position + Vector3.up * (1f + m_radius), new Vector3(0.4f, 1f, 0.4f));
		Gizmos.DrawWireCube(transform.position + Vector3.up * (1f + m_radius), new Vector3(0.4f, 0.4f, 1f));
	}
}
