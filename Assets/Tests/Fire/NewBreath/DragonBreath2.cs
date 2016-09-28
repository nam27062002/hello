using UnityEngine;
using System.Collections;

public class DragonBreath2 : MonoBehaviour 
{

	// Mesh cache
	private int[] m_triangles = null;
	private Vector3[] m_pos = null;
	private Vector2[] m_UV = null;
    private Color[] m_color = null;

    // Meshes
    private Mesh m_mesh = null;

	// Cached components
	private MeshFilter m_meshFilter = null;

    public float m_distance = 10;
    public float m_aplitude = 6;
    private float m_splits = 5;

    private int m_numPos = 0;

    Vector3[] m_whip;
    Vector3[] m_whipTangent;
//    bool[] m_whipCollision;

    private int m_collisionSplit = 0;

    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;

    public AnimationCurve m_shapeCurve;
    public AnimationCurve m_FlameAnimation;
    public AnimationCurve m_FlexCurve;

    private float flameAnimationTime = 0.0f;

    public float fireDelay = 1.0f;

    public string m_groundLayer;
    public string[] m_enemyLayers;

    private int m_groundLayerMask;


    private Vector3 lastInitialPosition;
    private Transform whipEnd;
    private Transform iniCanon;

    public float timeDelay = 0.25f;

    private float lastTime;

    private float enableTime = 0.0f;
    private bool enableState = false;

    private Vector3[] originalVertices;
    private Vector2[] originalUV;

    private Color[] colorVertex;

    public Vector3 CanonScale = Vector3.one;



    // Use this for initialization
    void Start () 
	{

        PoolManager.CreatePool((GameObject)Resources.Load("Particles/Fire&Destruction/_PrefabsWIP/FireOfBreath"), 15, false);

        whipEnd = transform.FindTransformRecursive("WhipEnd");
        iniCanon = transform.FindTransformRecursive("IniCanon");
        // Cache
        lastInitialPosition = whipEnd.position;

//        flameAnimationTime = m_FlameAnimation[m_FlameAnimation.length - 1].time;

//        enableTime = lastTime = Time.time;

        initMesh();
	}

    void initMesh()
    {

        m_meshFilter = iniCanon.GetComponent<MeshFilter>();

        m_mesh = m_meshFilter.sharedMesh;
        m_mesh.MarkDynamic();

        colorVertex = new Color[m_mesh.vertices.Length];// m_mesh.colors;
        originalVertices = m_mesh.vertices;

        for (int c = 0; c < colorVertex.Length; c++)
        {
            if (c < 4)
            {
                colorVertex[c] = m_initialColor;
            }
            else if (c > 8)//(colorVertex.Length - 20))
            {
                colorVertex[c] = m_collisionColor;
            }
            else
            {
                colorVertex[c] = Color.white;
            }

            originalVertices[c] = Vector3.Scale(originalVertices[c], CanonScale);
        }

        m_mesh.normals = null;
        m_mesh.vertices = originalVertices;
        m_mesh.colors = colorVertex;
        m_meshFilter.sharedMesh = m_mesh;

    }

    // Update is called once per frame
    void Update () 
	{
        Vector3 front = transform.InverseTransformDirection(Vector3.forward);
        iniCanon.localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Vector3.Dot(front, Vector3.right), Vector3.up);
//        iniCanon.Rotate( Vector3.up, , Space.Self);
//        iniCanon.rotation = Quaternion.
	}

/*
    public void EnableFlame(bool value)
    {
        if (value)
        {
            gameObject.active = value;
        }

        enableTime = Time.time;
        enableState = value;
    }

*/

    public void EnableFlame(bool value)
    {
        gameObject.active = value;
        enableTime = Time.time;
        enableState = value;
    }

}
