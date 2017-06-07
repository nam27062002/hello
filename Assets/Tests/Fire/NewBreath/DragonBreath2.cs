using UnityEngine;
using System.Collections;

public class DragonBreath2 : MonoBehaviour 
{
	// Mesh cache
	private int[] m_triangles = null;
	private Vector3[] m_pos = null;
	private Vector2[] m_UV = null;
    private Color[] m_color = null;

    private Vector3[] m_whip;
    private Vector3[] m_whipTangent;
    //    bool[] m_whipCollision;

    // Meshes
    private Mesh m_mesh = null;

	// Cached components
	private MeshFilter m_meshFilter = null;

    public float m_distance = 10;
    public float m_aplitude = 6;
    private float m_splits = 5;

    private int m_numPos = 0;

    private int m_collisionSplit = 0;

    public string m_collisionFirePrefab;
    public float m_collisionFireDelay = 0.5f;
    
    public string m_groundLayer;
    public string[] m_enemyLayers;

    private int m_groundLayerMask;

    private Transform m_whipEnd;
    private Transform m_iniCanon;

    private float m_lastTime;
    private bool m_enableState = false;

    private Vector3[] m_originalVertices;
    private Vector2[] m_originalUV;

    private Color[] m_colorVertex;

    public Vector3 m_canonScale = Vector3.one;

    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;


	private ParticleHandler m_collisionFireHandler;

    // Use this for initialization
    void Start () 
	{
		m_collisionFireHandler = ParticleManager.CreatePool(m_collisionFirePrefab, "Fire&Destruction/_PrefabsWIP/FireEffects/");

        Transform t = transform;
        m_whipEnd = t.FindTransformRecursive("WhipEnd");
        m_iniCanon = t.FindTransformRecursive("IniCanon");

        m_groundLayerMask = LayerMask.GetMask(m_groundLayer);

        //        enableTime = lastTime = Time.time;
        initMesh();

        m_lastTime = Time.time;
    }

    void initMesh()
    {

        m_meshFilter = m_iniCanon.GetComponent<MeshFilter>();

        m_mesh = m_meshFilter.sharedMesh;
        m_mesh.MarkDynamic();

        m_colorVertex = new Color[m_mesh.vertices.Length];// m_mesh.colors;
        m_originalVertices = m_mesh.vertices;

        for (int c = 0; c < m_colorVertex.Length; c++)
        {
            if (c < 4)
            {
                m_colorVertex[c] = m_initialColor;
            }
            else if (c > 8)//(colorVertex.Length - 20))
            {
                m_colorVertex[c] = m_collisionColor;
            }
            else
            {
                m_colorVertex[c] = Color.white;
            }

            m_originalVertices[c] = Vector3.Scale(m_originalVertices[c], m_canonScale);
        }

        m_mesh.normals = null;
        m_mesh.vertices = m_originalVertices;
        m_mesh.colors = m_colorVertex;
        m_meshFilter.sharedMesh = m_mesh;

    }

    // Update is called once per frame
    void Update()
    {

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, m_distance, m_groundLayerMask))
        {
            if (Time.time > m_lastTime + m_collisionFireDelay)
            {
				GameObject colFire = m_collisionFireHandler.Spawn(null, hit.point);
                if (colFire != null)
                {
                    colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
                }

                m_lastTime = Time.time;
            }
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position - transform.up * m_distance, 0.25f);
    }

    public void EnableFlame(bool value)
    {
        gameObject.SetActive(value);
    }

}
