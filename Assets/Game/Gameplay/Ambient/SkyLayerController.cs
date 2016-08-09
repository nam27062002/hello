using UnityEngine;
using System.Collections;

public class SkyLayerController : MonoBehaviour 
{

	public float m_MoveProportion = 1000.0f;
	public float m_scrollSpeed = 0.1f;

	public float m_minYDark = 50;
	public float m_maxYDark = 200;
	public Color darkColor = new Color(0.1f, 0.0f, 0.3f);

	private Material m_material;
	private Vector2 m_offset = Vector3.zero;
	private Transform m_playerTransform;

	// Use this for initialization
	IEnumerator Start() 
	{
		while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
		{
			yield return null;
		}


		m_playerTransform = InstanceManager.player.transform;
		m_material = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		if ( m_playerTransform )
		{
			Vector3 pos = m_playerTransform.transform.position;
			m_material.SetFloat("_Scroll2X", m_scrollSpeed);
			m_offset.x = pos.x / m_MoveProportion;
			m_offset.y = pos.y / m_MoveProportion;
			m_material.SetTextureOffset( "_DetailTex", m_offset);

			if ( pos.y > m_minYDark )
			{
				float delta = Mathf.Clamp01( (pos.y - m_minYDark) / ( m_maxYDark - m_minYDark ) );
				m_material.SetColor( "_Color", Color.Lerp( Color.white, darkColor, delta) );
			}
			else
			{
				m_material.SetColor( "_Color", Color.white);	
			}
		}
	}
}
