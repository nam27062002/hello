using UnityEngine;
using System.Collections;

public class FireBreathDynamic : MonoBehaviour 
{

	// Mesh cache
	private int[] m_triangles = null;
	private Vector3[] m_pos = null;
	private Vector2[] m_UV = null;

	// Meshes
	private Mesh m_mesh = null;


	// Cached components
	private MeshFilter m_meshFilter = null;

    private float m_distance = 20;
    private float m_aplitude = 6;
    private float m_splits = 10;

    private int m_numPos = 0;

    Vector3[] m_whip;

	// Use this for initialization
	void Start () 
	{
		// Cache
		m_meshFilter = GetComponent<MeshFilter>();
		m_numPos = (int)(4 + m_splits * 2);

		InitWhip();
		InitArrays();
		InitUVs();
		InitTriangles();

		ReshapeFromWhip();

        CreateMesh();

	}

	void InitWhip()
	{
		m_whip = new Vector3[(int)m_splits + 1];
		float xStep = m_distance / (m_splits + 1);
		Vector3 move = transform.right.normalized;
		Vector3 pos = transform.position;
		for( int i = 0; i<m_splits + 1; i++ )
		{
			m_whip[i] = pos + (move * xStep * i);
		}
	}

	void InitArrays()
	{	
		m_pos = new Vector3[m_numPos];
		m_UV = new Vector2[m_numPos];
		for( int i = 0;i<m_numPos; i++ )
		{
			m_pos[i] = Vector3.zero;
			m_UV[i] = Vector2.zero;
		}
	}

	void InitUVs()
	{
		m_UV[0] = Vector2.zero;
		m_UV[1] = Vector2.right;
		float vStep = 1.0f / (m_splits + 1);

		int step = 1;
		for( int i = 2; i<m_numPos; i += 2 )
		{
			m_UV[i].y = vStep * step;

			m_UV[i+1].x = 1;
			m_UV[i+1].y = vStep * step;

			step++;
		}

		// m_UV[m_numPos - 2 ] = Vector2.up;
		// m_UV[ m_numPos - 1 ] = Vector2.one;
	}

	void InitTriangles()
	{
		int numTrianglesIndex = (m_numPos-2) * 3;
		m_triangles = new int[ numTrianglesIndex ];

		int pos = 0;
		for( int i = 0; i<numTrianglesIndex; i += 3 )
		{
			m_triangles[i] = pos;
			if ( pos % 2 == 0 )
			{
				m_triangles[i+1] = pos+2;
				m_triangles[i+2] = pos+1;

			}
			else
			{
				m_triangles[i+1] = pos+1;
				m_triangles[i+2] = pos+2;
			}
			pos++;
		}
	}

	void Reshape( /* float angle? */)
	{
		m_pos[0] = Vector3.zero;
		m_pos[1] = Vector3.zero;

		float xStep = m_distance / (m_splits + 1);
		float yStep = m_aplitude / (m_splits + 1);

		int step = 1;
		for( int i = 2; i<m_numPos; i += 2 )
		{
			m_pos[i].x = xStep * step;
			m_pos[i].y = yStep * step;

			m_pos[i+1].x = xStep * step;
			m_pos[i+1].y = -yStep * step;
			step++;
		}

		// m_pos[m_numPos - 2] = Vector3.right * m_distance + Vector3.up * m_aplitude;
		// m_pos[m_numPos - 1] = Vector3.right * m_distance + Vector3.down * m_aplitude;
	}


	void ReshapeFromWhip( /* float angle? */)
	{
		m_pos[0] = Vector3.zero;
		m_pos[1] = Vector3.zero;

		// float xStep = m_distance / (m_splits + 1);
		float yStep = m_aplitude / (m_splits + 1);

		Vector3 worldPos = transform.position;
		int step = 1;
		int whipIndex = 0;

		for( int i = 2; i<m_numPos; i += 2 )
		{

			Vector3 normal = m_whip[whipIndex];
			if ( whipIndex > 0 )
				normal = m_whip[whipIndex] - m_whip[whipIndex-1];
			else
				normal = m_whip[whipIndex] - transform.position;
			normal.Normalize();
			float tmp = normal.x;
			normal.x = -normal.y;
			normal.y = tmp;

			normal = transform.InverseTransformDirection( normal );

			m_pos[i] =  transform.InverseTransformPoint(m_whip[whipIndex]);
			m_pos[i] += normal * yStep * step;
			m_pos[i].z = 0;

			m_pos[i+1] = transform.InverseTransformPoint(m_whip[whipIndex]);
			m_pos[i+1] += normal * -yStep * step;
			m_pos[i+1].z = 0;

			step++;
			whipIndex++;
		}

		// m_pos[m_numPos - 2] = Vector3.right * m_distance + Vector3.up * m_aplitude;
		// m_pos[m_numPos - 1] = Vector3.right * m_distance + Vector3.down * m_aplitude;
	}


	// Recreates the mesh
	void CreateMesh()
	{
		m_mesh = new Mesh();
		m_mesh.MarkDynamic();

		m_mesh.vertices = m_pos;
        m_mesh.uv = m_UV;

        m_mesh.SetTriangles( m_triangles, 0);
		// m_mesh.SetIndices(m_triangles, MeshTopology.Triangles, 0);
        m_meshFilter.sharedMesh = m_mesh;

	}

	
	// Update is called once per frame
	void Update () 
	{
		MoveWhip();
		ReshapeFromWhip();
		m_mesh.vertices = m_pos;
	}

	void MoveWhip()
	{
		float xStep = m_distance / (m_splits + 1);
		Vector3 move = transform.right.normalized;
		Vector3 pos = transform.position;
		for( int i = 0; i<m_splits + 1; i++ )
		{
			Vector3 shouldBePos = pos + move * xStep * (i+1);
			m_whip[i] = Vector3.Lerp( m_whip[i], shouldBePos, (1.1f - (i/m_splits)) * Time.deltaTime * 10);
		}
	}
}
