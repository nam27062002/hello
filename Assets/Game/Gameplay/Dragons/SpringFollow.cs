using UnityEngine;

public class SpringFollow : MonoBehaviour
{
    [SerializeField]
    Transform m_target;

    [SerializeField]
    Vector3 m_offset;

    [SerializeField]
    float m_smoothTime = 0.5f;

    Vector3 m_velocity = Vector3.zero;

    void LateUpdate()
    {
        if (m_target == null)
            return;

        Vector3 targetPosition = m_offset + m_target.position;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_velocity, m_smoothTime);
    }
}
