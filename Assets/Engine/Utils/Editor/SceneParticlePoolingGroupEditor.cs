using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneParticlePoolingGroup))]
public class SceneParticlePoolingGroupEditor : Editor {

	SceneParticlePoolingGroup m_target = null;

	static List<ParticleDataPlace> m_particlesCreated = new List<ParticleDataPlace>();
	static List<GameObject> m_particlesCreatedObj = new List<GameObject>();

	private void OnEnable() {
		if ( !Application.isPlaying ){
			m_target = target as SceneParticlePoolingGroup;
			CreateTargetParticles( m_target );
		}
	}

	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
		if ( GUILayout.Button("Recreate Particle") )
		{
			DestroyTargetParticles( m_target );
			CreateTargetParticles( m_target );
		}
        if ( GUILayout.Button("Create All Particles") )
        {
			SceneParticlePoolingGroup[] p = Object.FindObjectsOfType<SceneParticlePoolingGroup>();
			for( int i = 0; i<p.Length; i++ )
			{
				CreateTargetParticles( p[i] );
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

	static void CreateTargetParticles(SceneParticlePoolingGroup target)
    {
		ParticleDataPlace[] particles = target.GetParticleDataPlaces();
		for( int i = 0; i<particles.Length; ++i )
		{
			if (!m_particlesCreated.Contains( particles[i] ))
			{
				CreateInstance( particles[i] );
			}
		}
    }

	static void DestroyTargetParticles(SceneParticlePoolingGroup target)
    {
		ParticleDataPlace[] particles = target.GetParticleDataPlaces();
		for( int i = 0; i<particles.Length; ++i )
		{
			for( int j = 0; j<m_particlesCreated.Count; j++ )
			{
				if (m_particlesCreated[j] == particles[i])
				{
					m_particlesCreated.RemoveAt(i);
					DestroyImmediate( m_particlesCreatedObj[i] );
					m_particlesCreatedObj.RemoveAt(i);	
					break;
				}
			}
		}
    }


   	static void CreateInstance( ParticleDataPlace particle )
   	{
		GameObject _particleInstance = particle.m_particle.CreateInstance();
		if (_particleInstance != null) {
			// As children of ourselves
			// Particle system should already be created to match the zero position
			_particleInstance.transform.SetParentAndReset(particle.transform);
			_particleInstance.transform.position += particle.m_particle.offset;
			_particleInstance.transform.rotation = particle.transform.rotation;
			_particleInstance.hideFlags = HideFlags.DontSaveInEditor;

			ParticleControl pc = _particleInstance.GetComponent<ParticleControl>();
			if (pc != null) {
				pc.Play(particle.m_particle);
			}
			else
			{
				ParticleScaler scaler = _particleInstance.GetComponent<ParticleScaler> ();
				if (scaler != null) {
					if (scaler.m_scale != particle.m_particle.scale) {
						scaler.m_scale = particle.m_particle.scale;
						scaler.DoScale();
					}
				}
			}

			m_particlesCreated.Add( particle );
			m_particlesCreatedObj.Add( _particleInstance );
		}
   	}

}
