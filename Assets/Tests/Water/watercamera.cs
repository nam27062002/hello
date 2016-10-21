using UnityEngine;
using System.Collections;

public class watercamera : MonoBehaviour {


    public float m_TurnSpeed = 10.0f;
    public float m_MoveSpeed = 10.0f;

    private Vector3 m_cameraEuler = Vector3.zero;
    private bool m_block = false;


    // Use this for initialization
    void Start () {
        //        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_block ^= true;
        }


        if (!m_block)
        {
            m_cameraEuler.x += Input.GetAxis("Mouse Y") * m_TurnSpeed * Time.deltaTime;
            m_cameraEuler.y += Input.GetAxis("Mouse X") * m_TurnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(m_cameraEuler);

            float yAxis = 0.0f;

            if (Input. GetKey(KeyCode.Q))
            {
                yAxis = -1.0f;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                yAxis = 1.0f;
            }
            transform.Translate(Input.GetAxis("Horizontal") * m_MoveSpeed * Time.deltaTime,
                                yAxis * m_MoveSpeed * Time.deltaTime,
                                Input.GetAxis("Vertical") * m_MoveSpeed * Time.deltaTime);
        }
	}
}
