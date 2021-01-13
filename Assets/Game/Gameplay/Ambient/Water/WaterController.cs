using UnityEngine;
using System.Collections;

public class WaterController : MonoBehaviour, IQuadTreeItem {
		
	private BoxCollider m_bounds;
	private Rect m_boundingRect;
	public Rect boundingRect { get { return m_boundingRect; } }

	// Use this for initialization
	void Start () {
		m_bounds = GetComponent<BoxCollider>();

		m_boundingRect = new Rect(transform.TransformPoint(m_bounds.center - m_bounds.size * 0.5f), new Vector2(transform.localScale.x * m_bounds.size.x, transform.localScale.y * m_bounds.size.y));

		WaterAreaManager.instance.Register(this);
	}
}
