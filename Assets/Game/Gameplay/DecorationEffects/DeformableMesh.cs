using UnityEngine;
using System.Collections;

public class DeformableMesh : MonoBehaviour 
{
	MeshFilter m_meshFilter;
	Mesh m_mesh;
	Vector3[] m_vertices;
	Vector3[] m_originalVertices;
	Collider m_other;

	public float m_forceMultiplier;
	public float m_distanceCheck;
	public float m_forceDuration = 1;

	Vector3 m_startForcrePosition;
	float m_forceTimer = 0;

	// Use this for initialization
	void Start () 
	{
		m_meshFilter = GetComponent<MeshFilter>();
		m_mesh = m_meshFilter.mesh;

		m_vertices = m_mesh.vertices;
		m_originalVertices = (Vector3[])m_vertices.Clone();


		m_other = null;
	}

	void UpdateMesh()
	{
		m_mesh.vertices = m_vertices;
	}

	// Update is called once per frame
	void Update () 
	{

		for( int i = 0; i<m_vertices.Length; i++ )
		{
			m_vertices[i] = m_originalVertices[i];
		}

		if ( m_forceTimer > 0 )
		{
			float delta = Mathf.Sin( m_forceTimer / m_forceDuration * Mathf.PI);
			// float delta = m_forceTimer / m_forceDuration;
			Deform( m_startForcrePosition, delta * m_forceMultiplier  , m_distanceCheck);
			m_forceTimer -= Time.deltaTime;

		}
		/*
		for( int i = 0; i<m_vertices.Length; i++ )
		{
			m_vertices[i] = Vector3.Lerp( m_vertices[i], m_originalVertices[i], Time.deltaTime * 2.0f);
		}
		*/

		UpdateMesh();
	}



	void OnTriggerEnter(Collider other) 
	{
		Debug.Log("OnTriggerEnter");
		m_other = other;
	}

	void OnTriggerStay(Collider other)
	{
		Debug.Log("OnTriggerStay");
	}

	void OnTriggerExit(Collider other) 
	{
		if (other is SphereCollider)
		{
			m_startForcrePosition = transform.InverseTransformPoint( other.transform.position );
			m_forceTimer = m_forceDuration;
		}
		Debug.Log("OnTriggerExit");
		/*
		m_other = other;
		if ( m_other != null )
		{
			Vector3 localPos = transform.InverseTransformPoint( m_other.transform.position );

		}
		*/
	}

	void Deform( Vector3 localOrigin, float force, float maxDistance)
	{
		float distance = maxDistance * maxDistance;
		for( int i = 0; i<m_vertices.Length; i++ )
		{
			Vector3 direction = (m_vertices[i] - localOrigin);
			float sq = direction.sqrMagnitude;
			if ( sq <= distance )
			{
				m_vertices[i] = m_vertices[i] + -direction.normalized * force * (sq / distance);
			}
		}
	}
}
