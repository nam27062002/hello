using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneParticlePooling))]
public class SceneParticlePoolingEditor : Editor {

	SceneParticlePooling m_target = null;
	static List<SceneParticlePooling> m_particlesCreated = new List<SceneParticlePooling>();
	static List<GameObject> m_particlesCreatedObj = new List<GameObject>();

	private void OnEnable() {
		if ( !Application.isPlaying ){
			m_target = target as SceneParticlePooling;
			if (!m_particlesCreated.Contains(m_target))
			{
				CreateInstance( m_target );
			}
		}
	}

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
		if ( GUILayout.Button("Recreate Particle") )
		{
			if ( m_particlesCreated.Contains( m_target ) )
			{
				for( int i = 0; i<m_particlesCreated.Count; i++ )
				{
					if (m_particlesCreated[i] == m_target)
					{
						m_particlesCreated.RemoveAt(i);
						DestroyImmediate( m_particlesCreatedObj[i] );
						m_particlesCreatedObj.RemoveAt(i);	
						break;
					}
				}
			}
			CreateInstance( m_target );
		}
        if ( GUILayout.Button("Create All Particles") )
        {
			SceneParticlePooling[] p = Object.FindObjectsOfType<SceneParticlePooling>();
			for( int i = 0; i<p.Length; i++ )
			{
				if ( !m_particlesCreated.Contains( p[i] ) )
				{
					CreateInstance( p[i] );
				}
			}

        }
        	
		if ( GUILayout.Button("Remove All Particles"))
		{
			m_particlesCreated.Clear();
			for( int i = 0; i<m_particlesCreatedObj.Count; i++ )
			{
				DestroyImmediate( m_particlesCreatedObj[i] );
			}
			m_particlesCreatedObj.Clear();
		}
    }


   	static void CreateInstance( SceneParticlePooling particleObj )
   	{
		GameObject _particleInstance = particleObj.m_particle.CreateInstance();
		if (_particleInstance != null) {
			// As children of ourselves
			// Particle system should already be created to match the zero position
			_particleInstance.transform.SetParentAndReset(particleObj.transform);
			_particleInstance.transform.position += particleObj.m_particle.offset;
			_particleInstance.transform.rotation = particleObj.transform.rotation;
			_particleInstance.hideFlags = HideFlags.DontSaveInEditor;
			ParticleSystem ps = _particleInstance.GetComponentInChildren<ParticleSystem>();

			ParticleScaler scaler = _particleInstance.GetComponent<ParticleScaler> ();
			if (scaler != null) {
				if (scaler.m_scale != particleObj.m_particle.scale) {
					scaler.m_scale = particleObj.m_particle.scale;
					scaler.DoScale();
				}
			}



			m_particlesCreated.Add( particleObj );
			m_particlesCreatedObj.Add( _particleInstance );
		}
   	}

}
