using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastLineRenderer : MonoBehaviour {

    [SerializeField] private Vector3[] m_points = null;
    [SerializeField] private float m_size;
    public float size { get { return m_size; } set { m_size = value; }}

    [SerializeField] private Transform[] m_3dSegments = null;

    private Transform m_transform;

    private void Awake() {
        m_transform = transform;
        UpdateSegments();
    }

    public void UpdateSegments() {
        for (int i = 0; i < m_points.Length - 1; ++i) {
            Vector3 d = m_points[i + 1] - m_points[i];
            float s = d.magnitude;
            d.Normalize();

            m_3dSegments[i].position = m_points[i];
            m_3dSegments[i].localScale = new Vector3(m_size, m_size, s);
            m_3dSegments[i].rotation = Quaternion.LookRotation(d);
        }
    }

    public void SetPoint(int _i, Vector3 _pos) {
        if (_i >= 0 && _i < m_points.Length) {
            m_points[_i] = _pos;
        }
    }

    private void OnDrawGizmosSelected() {
        m_transform = transform;
        UpdateSegments();

        for (int i = 0; i < m_points.Length - 1; ++i) {
            Gizmos.DrawLine(m_points[i], m_points[i + 1]);
        }
    }
}
