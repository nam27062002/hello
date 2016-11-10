using UnityEngine;
using System.Collections;

public class WaterMesh : MonoBehaviour 
{
	public float m_cellSpacing = 5.0f;
	
	private Transform m_transform = null;
	private float m_width;
	private float m_length;
    private float m_height;
    private int m_numVertices;
	private int m_numTriangles;
    private int m_numTriangles2;

    private Mesh m_mesh;
	private Vector3[] m_vertices;
	private Vector2[] m_UV;
	private Vector2[] m_UV2;
    private Color[] m_colours;

    private int[] m_indices;
    private int[] m_indices2;
	
	private Vector3 m_position;


    public bool generateMesh = true;
	
	void Awake()
	{
        if (!generateMesh)
        {
            return;
        }
        Bounds bounds = GetComponent<BoxCollider>().bounds;
		
		m_cellSpacing = (int)Mathf.Max(1, Mathf.Max(bounds.size.x * 0.1f, bounds.size.z * 0.1f));

		int numVertsX = (int)(bounds.size.x / m_cellSpacing);
		int numVertsZ = (int)(bounds.size.z / m_cellSpacing);

        if (numVertsX == 0 || numVertsZ == 0) return;

        m_numVertices = (numVertsX * numVertsZ) + numVertsX * 2;
        m_numTriangles = (numVertsX - 1) * (numVertsZ - 1) * 2;
        m_numTriangles2 = (numVertsX - 1) * 2;

        m_vertices = new Vector3[m_numVertices];
        m_indices = new int[m_numTriangles * 3];
        m_indices2 = new int[m_numTriangles2 * 3];
        m_UV = new Vector2[m_numVertices];
        m_colours = new Color[m_numVertices];


        float uvspacing = 2.0f / m_cellSpacing;

        Vector3 min = bounds.min;// - transform.position;//bounds.center;
        Vector3 max = bounds.max;// - transform.position;//bounds.center;

        int c = 0;
        for (int z = numVertsZ; z > 0; z--)
        {
            for (int x = 0; x < numVertsX; x++)
            {
                m_vertices[c] = transform.InverseTransformPoint(new Vector3(min.x + (x * m_cellSpacing), max.y, min.z + (z * m_cellSpacing)));
                m_UV[c] = new Vector2(-z * uvspacing, x * uvspacing);
                m_colours[c++] = Color.gray;
            }
        }


        for (int x = 0; x < numVertsX; x++)
        {
            m_vertices[c] = transform.InverseTransformPoint(new Vector3(min.x + (x * m_cellSpacing), min.y, min.z + (m_cellSpacing)));
            m_UV[c] = new Vector2(1.0f * uvspacing, x * uvspacing);
            m_colours[c++] = Color.gray;
        }

        /*
                for (float x = min.x; x < max.x; x += m_cellSpacing)
                {
                    m_vertices[c] = new Vector3(x, min.y, min.z + (float)numVertsZ * m_cellSpacing);
                    m_UV[c] = new Vector2(x * uvspacing, uvspacing * 10.0f);
                    m_colours[c++] = Color.white;
                }
        */
        c = 0;

        for (int v = 0; v < numVertsZ - 1; v++)
        {
            for (int u = 0; u < numVertsX - 1; u++)
            {

                m_indices[c] = (numVertsX * v) + u;
                m_indices[c + 1] = (numVertsX * (v + 1)) + u;
                m_indices[c + 2] = (numVertsX * (v + 1)) + u + 1;

                m_indices[c + 3] = (numVertsX * v) + u;
                m_indices[c + 4] = (numVertsX * (v + 1)) + u + 1;
                m_indices[c + 5] = (numVertsX * v) + u + 1;


/*
                m_indices[c] = (numVertsX * v) + u;
                m_indices[c + 1] = (numVertsX * v) + u + 1;
                m_indices[c + 2] = (numVertsX * (v + 1)) + u;
                m_indices[c + 3] = (numVertsX * v) + u + 1;
                m_indices[c + 4] = (numVertsX * (v + 1)) + u + 1;
                m_indices[c + 5] = (numVertsX * (v + 1)) + u;*/
                c += 6;
            }
        }

        c = 0;
        int v2 = numVertsZ - 1;
        for (int u = 0; u < numVertsX - 1; u++)
        {
            m_indices2[c] = (numVertsX * v2) + u;
            m_indices2[c + 1] = (numVertsX * v2) + u + 1;
            m_indices2[c + 2] = (numVertsX * (v2 + 1)) + u;
            m_indices2[c + 3] = (numVertsX * v2) + u + 1;
            m_indices2[c + 4] = (numVertsX * (v2 + 1)) + u + 1;
            m_indices2[c + 5] = (numVertsX * (v2 + 1)) + u;
            c += 6;
        }


    }

    // Use this for initialization
    void Start () 
	{
		m_transform = transform;
		m_position = m_transform.position;

        if (generateMesh)
        {
            m_mesh = GetComponent<MeshFilter>().mesh;
            m_mesh.Clear();

            m_mesh.vertices = m_vertices;
            m_mesh.colors = m_colours;
            m_mesh.uv = m_UV;
            m_mesh.uv2 = m_UV;
            //            m_mesh.triangles = m_indices;
            m_mesh.subMeshCount = 2;
            m_mesh.SetTriangles(m_indices, 0);
            m_mesh.SetTriangles(m_indices2, 1);
        }
    }
}
