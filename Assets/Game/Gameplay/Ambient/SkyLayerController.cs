using UnityEngine;
using System.Collections;

public class SkyLayerController : MonoBehaviour 
{

	public float m_MoveProportion = 1000.0f;

	private Material m_material;
	private Vector2 m_offset = Vector3.zero;
	private Transform m_playerTransform;

	Vector2 m_center;

	// Use this for initialization
	IEnumerator Start() 
	{
		while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded())
		{
			yield return null;
		}

		LevelData data = LevelManager.currentLevelData;
		if(data != null) {
			m_center = data.bounds.center;
		}

		m_playerTransform = InstanceManager.player.transform;
		m_material = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		if ( m_playerTransform )
		{
            Vector2 pos = (Vector2)m_playerTransform.transform.position;
            // m_material.SetFloat("_Scroll2X", m_scrollSpeed);
            m_offset.x = (pos.x - m_center.x) / m_MoveProportion;
            m_offset.y = (pos.y - m_center.y) / m_MoveProportion;
            m_material.SetTextureOffset( "_DetailTex", m_offset);
//            Vector2 pos = (Vector2)m_playerTransform.transform.position;
//            m_material.SetVector("_CamPos", pos);
		}
	}
}
