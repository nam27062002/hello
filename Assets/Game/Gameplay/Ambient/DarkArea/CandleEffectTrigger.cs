using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleEffectTrigger : MonoBehaviour {

//    public int m_id;
//    public float m_radius;
//    public float m_fallOff;

    [SerializeField] public HUDDarkZoneEffect.CandleData m_inData;
    [SerializeField] public HUDDarkZoneEffect.CandleData m_outData;

    [HideInInspector][SerializeField]
    public Transform m_tDirection;

    private Vector3 m_vDirection;
    public Vector3 Direction {
        get
        {
            return m_vDirection;
        }
    }

    private float m_vLength;
    public float Length
    {
        get
        {
            return m_vLength;
        }
    }

    void Awake()
    {
        m_tDirection = transform.Find("Direction").GetComponent<Transform>();
        m_vDirection = m_tDirection.position - transform.position;
        m_vLength = m_vDirection.magnitude;
        m_vDirection = m_vDirection.normalized;
    }

	private bool m_playerInside = false;

    void OnTriggerEnter( Collider other)
	{
		if (other.CompareTag("Player") && !m_playerInside) {
			Messenger.Broadcast<bool, CandleEffectTrigger> (MessengerEvents.DARK_ZONE_TOGGLE, true, this);
			m_playerInside = true;
		}
	}
    void OnTriggerExit(Collider other)
    {
		if (other.CompareTag("Player") && m_playerInside)
		{
			Messenger.Broadcast<bool, CandleEffectTrigger>(MessengerEvents.DARK_ZONE_TOGGLE, false, this);
			m_playerInside = false;
		}
    }


    public static void drawArrow(Vector3 p1, Vector3 p2, float arrowAngle, float arrowLength, Color col)
    {
        Gizmos.color = col;
        Gizmos.DrawLine(p1, p2);
        Vector3 n = Vector3.Normalize(p1 - p2);
        float s = Mathf.Sin(arrowAngle);
        float c = Mathf.Cos(arrowAngle);

        Vector3 d = new Vector3(c * n.x - s * n.y, s * n.x + c * n.y);
        Gizmos.DrawLine(p2, p2 + d * arrowLength);

        float dd = Vector3.Dot(d, -n);

        //        d.Set(-c * n.x + s * n.y, -s * n.x - c * n.y, 0.0f);
        d += n * dd * 2.0f;
        Gizmos.DrawLine(p2, p2 - d * arrowLength);
    }

    void OnDrawGizmosSelected()
    {
        if (m_tDirection != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, m_tDirection.position);

            drawArrow(transform.position, m_tDirection.position, Mathf.Deg2Rad * 25.0f, 2.0f, Color.cyan);

        }
    }
}
