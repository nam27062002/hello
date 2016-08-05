using UnityEngine;
using System.Collections;

public class SkyLayerController : MonoBehaviour 
{

	public float m_MoveProportion = 1000.0f;
	public float m_scrollSpeed = 0.1f;

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
			m_material.SetFloat("_Scroll2X", m_scrollSpeed);
			m_offset.x = m_playerTransform.transform.position.x / m_MoveProportion;
			m_offset.y = m_playerTransform.transform.position.y / m_MoveProportion;
			m_material.SetTextureOffset( "_DetailTex", m_offset);
		}
	}
}
