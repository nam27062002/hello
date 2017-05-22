using UnityEngine;
using System.Collections;

public class SkyLayerController : MonoBehaviour 
{

	public float m_MoveProportion = 1000.0f;
    public float m_MoveProportion2 = 1000.0f;

    private Material m_material;
//	private Vector2 m_offset = Vector3.zero;
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
            Vector2 offset = (pos - m_center) / m_MoveProportion;
            m_material.SetTextureOffset( "_DetailTex", offset);
            offset = (pos - m_center) / m_MoveProportion2;
            m_material.SetTextureOffset( "_MoonTex", offset);
        }
    }
}
