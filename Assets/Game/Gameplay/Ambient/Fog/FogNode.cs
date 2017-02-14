using UnityEngine;
using System.Collections;

public class FogNode : MonoBehaviour, IQuadTreeItem
{
	public Color m_fogColor = Color.white;
	public float m_fogStart = 0;
	public float m_fogEnd = 100;
	public float m_fogRamp = 1;

	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	void Awake()
	{
		m_rect = new Rect((Vector2)transform.position, Vector2.zero);
	}

	public void CustomGuizmoDraw( bool used = false)
	{
		if ( used )
		{
			Gizmos.color = Color.white;
			Gizmos.DrawSphere( transform.position, 1.25f);	
		}
		Gizmos.color = m_fogColor;
		Gizmos.DrawSphere( transform.position, 1);
	}

	// public Transform transform { get {return gameObject.transform;} }

}
