using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class WaterMesh : MonoBehaviour 
{
    public WaterController m_waterController = null;
	public float m_cellSize = 5.0f;
	
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

    private Vector3 m_colliderCenter;
    private Vector3 m_colliderSize;

    private Material m_overWaterMaterial;

    public bool generateMesh = true;
	
	void Awake()
	{
        MeshRenderer mr = GetComponent<MeshRenderer>();
        m_overWaterMaterial = mr.materials[0];

        if (!generateMesh)
        {
            return;
        }
        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 center = box.center;
        Vector3 size = box.size;
        Bounds bounds = box.bounds;
        Vector3 lscale = transform.localScale;

        //		m_cellSpacing = (int)Mathf.Max(1, Mathf.Max(bounds.size.x * 0.1f, bounds.size.z * 0.1f));

        Vector3 min = bounds.min;// - transform.position;//bounds.center;
        Vector3 max = bounds.max;// - transform.position;//bounds.center;
//        Vector3 lscale = transform.localScale;
        transform.SetLocalScale(1.0f);

        int numVertsX = (int)(bounds.size.x / m_cellSize);
		int numVertsZ = (int)(bounds.size.z / m_cellSize);

        if (numVertsX < 2 || numVertsZ < 2) return;

        m_numVertices = (numVertsX * numVertsZ) + numVertsX * 2;
        m_numTriangles = (numVertsX - 1) * (numVertsZ - 1) * 2;
        m_numTriangles2 = (numVertsX - 1) * 2;

        m_vertices = new Vector3[m_numVertices];
        m_indices = new int[m_numTriangles * 3];
        m_indices2 = new int[m_numTriangles2 * 3];
        m_UV = new Vector2[m_numVertices];
        m_colours = new Color[m_numVertices];

        float uvspacing = 2.0f / m_cellSize;

//        Vector3 min = bounds.min;// - transform.position;//bounds.center;
//        Vector3 max = bounds.max;// - transform.position;//bounds.center;

        int c = 0;
        for (int z = numVertsZ; z > 0; z--)
        {
            for (int x = 0; x < numVertsX; x++)
            {
                m_vertices[c] = transform.InverseTransformPoint(new Vector3(min.x + (x * m_cellSize), max.y, min.z + (z * m_cellSize)));
                m_UV[c] = new Vector2(-z * uvspacing, x * uvspacing);
                m_colours[c++] = Color.gray;
            }
        }

        for (int x = 0; x < numVertsX; x++)
        {
            m_vertices[c] = transform.InverseTransformPoint(new Vector3(min.x + (x * m_cellSize), min.y, min.z + (m_cellSize)));
//            m_UV[c] = new Vector2(1.0f * uvspacing * min.y, x * uvspacing);
            m_UV[c] = new Vector2(1.0f, x * uvspacing);
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

        //        min.Set(lscale.x / min.x, lscale.y / min.y, lscale.z / min.z);
        //        max.Set(lscale.x / max.x, lscale.y / max.y, lscale.z / max.z);
        m_colliderCenter.Set(center.x * lscale.x, center.y * lscale.y, center.z * lscale.z);
        m_colliderSize.Set(size.x * lscale.x, size.y * lscale.y, size.z * lscale.z);
//        box.bounds.SetMinMax(min, max);

		box.center = m_colliderCenter;
		box.size = m_colliderSize;

    }

    // Use this for initialization
    void Start () 
	{
        // Please reimplement HasBeenStarted() method below if you move this statement since we're assuming that m_transform is null until this Start() is called
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

        Messenger.AddListener<bool>(GameEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);

    }

    private bool HasBeenStarted()
    {
        return m_transform != null;
    }

    void OnDestroy()
    {
        // The listener has to be removed only if it was added, since it's added in Start() we need to check if it's been started
        if (HasBeenStarted())
        {
            Messenger.RemoveListener<bool>(GameEvents.UNDERWATER_TOGGLED, OnUnderwaterToggled);
        }
    }    

    private void OnUnderwaterToggled(bool _activated)
    {

        if (m_overWaterMaterial != null)
        {
            Vector3 startPosition = m_transform.InverseTransformPoint(InstanceManager.player.transform.position);
            m_overWaterMaterial.SetFloat("_StartTime", Time.timeSinceLevelLoad);
            m_overWaterMaterial.SetVector("_StartPosition", startPosition);
//            m_overWaterMaterial.SetFloat("_WaterSpeed", Random.RandomRange(1.0f, 3.0f));
        }
        //        Debug.Log("WaterMesh - OnUnderwaterToggled " + playerLocation);



    }

}
