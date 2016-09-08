using UnityEngine;
using System.Collections;

public class FogNode : MonoBehaviour, IQuadTreeItem
{
	public Color m_fogColor = Color.white;
	public float m_fogStart = 0;
	public float m_fogEnd = 100;

	private Rect m_rect;
	public Rect boundingRect { get { 
										if (m_rect == null) {
											m_rect = new Rect((Vector2)transform.position, Vector2.zero);
										}
										return m_rect; } 
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
