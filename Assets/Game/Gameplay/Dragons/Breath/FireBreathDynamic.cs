using UnityEngine;
using System.Collections;

public class FireBreathDynamic : MonoBehaviour 
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
    private float m_splits = 10;

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


    public float fireDelay = 1.0f;

    public string m_groundLayer;
    public string[] m_enemyLayers;

    private int m_groundLayerMask;


    private Vector3 lastInitialPosition;
    private GameObject whipEnd;

//    public FireOfBreathScript breathFire = null;
    public GameObject breathFire = null;
    public float timeDelay = 0.25f;

    private float lastTime;

	// Use this for initialization
	void Start () 
	{
		// Cache
		m_meshFilter = GetComponent<MeshFilter>();
		m_numPos = (int)(4 + m_splits * 2);

        m_groundLayerMask = LayerMask.GetMask(m_groundLayer);

        whipEnd = transform.FindChild("WhipEnd").gameObject;
        whipEnd.transform.SetLocalPosX(m_distance);

        InitWhip();
		InitArrays();
		InitUVs();
		InitTriangles();

		ReshapeFromWhip();

        CreateMesh();

        lastInitialPosition = whipEnd.transform.position;

        lastTime = Time.time;
	}

	void InitWhip()
	{
		m_whip = new Vector3[(int)m_splits + 1];
        m_whipTangent = new Vector3[(int)m_splits + 1];
//        m_whipCollision = new bool[(int)m_splits + 1];

        float xStep = m_distance / (m_splits + 1);
		Vector3 move = transform.right;
		Vector3 pos = transform.position;
		for( int i = 0; i < (m_splits + 1); i++ )
		{
			m_whip[i] = pos + (move * xStep * i);
            m_whipTangent[i] = transform.up;
//            m_whipCollision[i] = false;

        }
	}
	void InitArrays()
	{	
		m_pos = new Vector3[m_numPos];
		m_UV = new Vector2[m_numPos];
        m_color = new Color[m_numPos];
        for ( int i = 0; i < m_numPos; i++ )
		{
			m_pos[i] = Vector3.zero;
			m_UV[i] = Vector2.zero;
            m_color[i] = (i < 4) ? m_initialColor : m_flameColor;
		}
	}

	void InitUVs()
	{
		m_UV[0] = Vector2.right * 0.5f;
		m_UV[1] = Vector2.right * 0.5f;
		float vStep = 1.0f / (m_splits + 1);
		float hStep = 1.0f / (m_splits + 1);

		int step = 1;
		for( int i = 2; i<m_numPos; i += 2 )
		{
			float xDisplacement = m_shapeCurve.Evaluate(step/(float)(m_splits+2)) * 0.5f;

			//m_UV[i].x = 0.5f + hStep/2.0f * step;
			m_UV[i].x = 0.5f + xDisplacement;
			m_UV[i].y = vStep * step;

			// m_UV[i+1].x = 0.5f - hStep/2.0f * step;
			m_UV[i+1].x = 0.5f - xDisplacement;
			m_UV[i+1].y = vStep * step;

			step++;
		}
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
    /*
	void Reshape( )
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

    */
	void ReshapeFromWhip( /* float angle? */)
	{
		m_pos[0] = m_pos[1] = Vector3.zero;

		int step = 1;
		int whipIndex = 0;
        Vector3 newPos1, newPos2;
//        Vector3 WhipExtreme = 

        for ( int i = 2; i<m_numPos; i += 2 )
		{
			float yDisplacement = m_shapeCurve.Evaluate(step/(float)(m_splits+2)) * m_aplitude;
            Vector3 whipTangent = transform.InverseTransformDirection(m_whipTangent[whipIndex]);

            newPos1 = newPos2 = transform.InverseTransformPoint(m_whip[whipIndex]);

            float kd = m_FlexCurve.Evaluate(whipIndex / m_splits);

            float md = 0.0f;// (whipEnd.transform.position.y - lastInitialPosition.y) * kd * fireDelay;


                        if (transform.right.x < 0.0f)
                        {
                            newPos1 += (whipTangent) * (yDisplacement + md);
                            newPos2 -= (whipTangent) * (yDisplacement - md);
                        }
                        else
                        {
                            newPos1 += (whipTangent) * (yDisplacement - md);
                            newPos2 -= (whipTangent) * (yDisplacement + md);
                        }


            m_pos[i] = newPos1;
            m_pos[i + 1] = newPos2;

            if (i > 2)
            {
                m_color[i] = m_color[i + 1] = (whipIndex > m_collisionSplit) ? m_collisionColor : m_flameColor;
            }

            step++;
			whipIndex++;
		}

//        Debug.Log("lastInitialPositionVariation: " + (lastInitialPosition - transform.position).ToString());

        lastInitialPosition = whipEnd.transform.position;


    }


    // Recreates the mesh
    void CreateMesh()
	{
		m_mesh = new Mesh();
		m_mesh.MarkDynamic();

		m_mesh.vertices = m_pos;
        m_mesh.uv = m_UV;
        m_mesh.colors = m_color;


        m_mesh.SetTriangles( m_triangles, 0);
		// m_mesh.SetIndices(m_triangles, MeshTopology.Triangles, 0);
        m_meshFilter.sharedMesh = m_mesh;

	}

	
	// Update is called once per frame
	void Update () 
	{
//        MoveWhip();
        UpdateWhip();
		ReshapeFromWhip();
		InitUVs();

		m_mesh.uv = m_UV;
		m_mesh.vertices = m_pos;
        m_mesh.colors = m_color;
	}



    void UpdateWhip()
    {
        RaycastHit hit;
        Vector3 whipDirection = transform.right;
        whipDirection.z = 0.0f;
        whipDirection.Normalize();
        //        Vector3 whipTangent = transform.up;

        Vector3 whipTangent = Vector3.Cross(Vector3.forward, whipDirection);//transform.up;

        Vector3 whipOrigin = transform.position;

        float flameAnim = m_FlameAnimation.Evaluate(Time.realtimeSinceStartup);

        float xStep = (flameAnim * m_distance) / (m_splits + 1);
        m_collisionSplit = (int)m_splits + 1;

        if (Physics.Raycast(transform.position, transform.right, out hit, m_distance, m_groundLayerMask))
        {
            Vector3 hitNormal = hit.normal;
            float wn = Vector3.Dot(hitNormal, whipDirection);
            Vector3 whipReflect = whipDirection - (hitNormal * wn * 2.0f);
//            Vector3 whipReflectTangent = Vector3.Cross(whipReflect, (whipDirection.x < 0.0f) ? -Vector3.forward: Vector3.forward);
            Vector3 whipReflectTangent = Vector3.Cross(Vector3.forward, whipReflect);
            //            Vector3 whipReflectTangent = Vector3.Cross(whipReflect, Vector3.forward);

            //            Instantiate<FireOfBreathScript>(breathFire)

            if (Time.time > lastTime + timeDelay)
            {
                Instantiate(breathFire, hit.point, Quaternion.AngleAxis(Random.value * 360.0f, Vector3.forward));
                lastTime = Time.time;
            }

            for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * i);

                if (currentDist < hit.distance)
                {
                    m_whip[i] = whipOrigin + (whipDirection * currentDist);
                    m_whipTangent[i] = whipTangent;
//                    m_whipCollision[i] = false;
                }
                else if (currentDist < (hit.distance + xStep))
                {
                    m_whip[i] = hit.point;
//                    m_whipTangent[i] = (whipTangent + whipReflectTangent).normalized;
                    m_whipTangent[i] = whipTangent;    // (whipTangent + whipReflectTangent).normalized;
//                    m_whipCollision[i] = true;
                    m_collisionSplit = i;
                }
                else
                {
                    m_whip[i] = hit.point;
                    m_whipTangent[i] = whipTangent;

                    // (whipTangent + whipReflectTangent).normalized;
                    //                    m_whip[i] = hit.point + ((currentDist - hit.distance) * whipReflect);
                    //                    m_whipTangent[i] = whipReflectTangent;
                    //                    m_whipCollision[i] = false;
                }

            }
        }
        else
        {
            for (int i = 0; i < m_splits + 1; i++)
            {
                float currentDist = (xStep * i);
                m_whip[i] = whipOrigin + (whipDirection * currentDist);
                m_whipTangent[i] = whipTangent;
//                m_whipCollision[i] = false;
            }
        }

//        MoveWhip();
    }


    void MoveWhip()
	{
		float xStep = m_distance / (m_splits + 1);
		Vector3 move = transform.right.normalized;
		Vector3 pos = transform.position;
		for( int i = 0; i<m_splits + 1; i++ )
		{
			Vector3 shouldBePos = pos + move * xStep * (i+1);
			m_whip[i] = Vector3.Lerp( m_whip[i], shouldBePos, (1.25f - (i/m_splits)) * Time.deltaTime * 15.0f);
		}
	}
}
