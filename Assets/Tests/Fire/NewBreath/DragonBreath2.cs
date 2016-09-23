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
    private GameObject whipEnd;

    public float timeDelay = 0.25f;

    private float lastTime;

    private float enableTime = 0.0f;
    private bool enableState = false;


	// Use this for initialization
	void Start () 
	{

        PoolManager.CreatePool((GameObject)Resources.Load("Particles/Fire/Prefabs/FireOfBreath"), 15, false);

        // Cache
        m_meshFilter = GetComponent<MeshFilter>();


        m_mesh = m_meshFilter.sharedMesh;
        m_mesh.MarkDynamic();
        Color[] colors = new Color[m_mesh.vertices.Length];// m_mesh.colors;

        for (int c = 0; c < colors.Length; c++)
        {
            colors[c] = new Color(Random.value, Random.value, Random.value);
        }

        m_mesh.colors = colors;

        m_meshFilter.sharedMesh = m_mesh;

        lastInitialPosition = whipEnd.transform.position;

        flameAnimationTime = m_FlameAnimation[m_FlameAnimation.length - 1].time;

        enableTime = lastTime = Time.time;


	}

	// Update is called once per frame
	void Update () 
	{
	}

    public void EnableFlame(bool value)
    {
        if (value)
        {
            gameObject.active = value;
        }

        enableTime = Time.time;
        enableState = value;
    }
}
