using UnityEngine;
using System.Collections;

public class WaterMesh : MonoBehaviour 
{
	private int m_cellSpacing;
	
	private Transform m_transform = null;
	private int m_numCellsPerRow;
	private int m_numCellsPerCol;
	private int m_width;
	private int m_depth;
	private int m_numVertices;
	private int m_numTriangles;
	
	private Mesh m_mesh;
	private Vector3[] m_vertices;
	private Vector2[] m_UV;
	private Vector2[] m_UV2;
	private int[] m_indices;
	private Color[] m_colours;
	
	private Vector3 m_position;

	
	void Awake()
	{
		BoxCollider bounds = GetComponent<BoxCollider>();
		
		m_cellSpacing = (int)Mathf.Max(1, Mathf.Max(bounds.size.x * 0.1f, bounds.size.z * 0.1f));

		int numVertsPerRow = (int)bounds.size.x / m_cellSpacing;
		int numVertsPerCol = (int)bounds.size.z / m_cellSpacing;

		MeshRenderer renderer = GetComponent<MeshRenderer>();
		renderer.material.SetFloat("_Width", bounds.size.y);
		renderer.material.SetFloat("_Near", bounds.size.y / 10f);
		renderer.material.SetFloat("_Far", bounds.size.y / 4f);
		renderer.material.SetFloat("_WaveAmplitude", m_cellSpacing / 10f);

		m_numCellsPerRow = numVertsPerRow - 1;
		m_numCellsPerCol = numVertsPerCol - 1;
		m_width = m_numCellsPerRow * m_cellSpacing;
		m_depth = m_numCellsPerCol * m_cellSpacing;
		m_numVertices = numVertsPerRow * numVertsPerCol;
		m_numTriangles = m_numCellsPerRow * m_numCellsPerCol * 2;
		
		
		m_vertices = new Vector3[m_numVertices];
		m_indices = new int[m_numTriangles * 3];
		m_UV = new Vector2[m_numVertices];
		m_UV2 = new Vector2[m_numVertices];
		m_colours = new Color[m_numVertices];
		
		int StartX = -(m_width / 2);
		int StartZ = 0;
		int EndX = m_width / 2;
		int EndZ = m_depth;

		float fUI = ((float)(m_numCellsPerRow) * 0.5f) / (float) m_numCellsPerRow;
		float fVI = ((float)(m_numCellsPerCol) * 0.5f) / (float) m_numCellsPerCol;
				
		int i = 0;
		float fDepth = 0.0f;

		float fZ = 0.0f;
		
		float fWaterStep = 0.0f;

		for(int z = StartZ; z <= EndZ; z += m_cellSpacing)
		{
			int j = 0;
			
			for(int x = StartX; x <= EndX; x += m_cellSpacing)
			{ 
				int iIndex = i * numVertsPerRow + j;
								
				float fX = (float)x;
				fZ = (float)z;
				
				m_vertices[iIndex].x = fX;				
				float fDamp;
				
				m_colours[iIndex].g = 1.0f;
				
				if(Mathf.Abs(fZ) < 1.0f && Mathf.Abs(fZ) >= 0.0f)
				{
					m_colours[iIndex].g = 0.0f;
					
				}
								
				m_colours[iIndex].r = Mathf.Sin(fWaterStep);
				fWaterStep += 0.01f + Random.Range(0.01f,0.02f);
				m_colours[iIndex].b = 1.0f;
				
				m_vertices[iIndex].y = 1.0f;				
				m_vertices[iIndex].z = fZ;
								
				if(z > fZ)
				{
					fZ = z;
				}
								
				m_UV[iIndex].x = j * fUI;
				m_UV[iIndex].y = i * fVI;
				m_UV2[iIndex].x = j * fUI;
				m_UV2[iIndex].y = i * fVI;
				
				++j;
			}
			
			fWaterStep += 0.3f + Random.Range(0.1f,1.4f);
			
			++i;
		}
		
		int iBaseIndex = 0;
		
		for(i = 0; i < m_numCellsPerCol; ++i)
		{
			for(int j = 0; j < m_numCellsPerRow; ++j)
			{
				
				m_indices[iBaseIndex] = i * numVertsPerRow + j;
				m_indices[iBaseIndex + 1] = i * numVertsPerRow + j + 1;
				m_indices[iBaseIndex + 2] = (i + 1) * numVertsPerRow + j;
				
				m_indices[iBaseIndex + 3] = (i + 1) * numVertsPerRow + j;
				m_indices[iBaseIndex + 4] = i * numVertsPerRow + j + 1;
				m_indices[iBaseIndex + 5] = (i + 1) * numVertsPerRow + j + 1;
				
 				iBaseIndex += 6;
			}
		}
		
	}

	// Use this for initialization
	void Start () 
	{
		m_transform = transform;
		m_position = m_transform.position;
	
		m_mesh = GetComponent<MeshFilter>().mesh;
		m_mesh.Clear();
			
		m_mesh.vertices = m_vertices;
		m_mesh.triangles = m_indices;
		m_mesh.colors = m_colours;
		m_mesh.uv = m_UV;
		m_mesh.uv2 = m_UV;
		m_position = m_transform.position;
	}
}
