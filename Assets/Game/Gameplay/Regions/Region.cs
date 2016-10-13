using System;
using UnityEngine;

[Serializable]
public class Region
{
    [SerializeField]
    protected string m_name;
    [SerializeField]
    protected float m_minX;
    [SerializeField]
    protected float m_maxX;
    [SerializeField]
    protected float m_minY;
    [SerializeField]
    protected float m_maxY;
    [SerializeField]
    protected int m_vertexAmount;
    [SerializeField]
    protected float[] m_vertexYs;
    [SerializeField]
    protected float[] m_multiples;
    [SerializeField]
    protected float[] m_constants;

    public bool Contains(float x, float y)
    {
        if (x >= m_minX && x < m_maxX && y >= m_minY && y < m_maxY)
        {
            var j = m_vertexAmount - 1;
            var inside = false;

            for (var i = 0; i < m_vertexAmount; i++)
            {
                if ((m_vertexYs[i] < y && m_vertexYs[j] >= y || m_vertexYs[j] < y && m_vertexYs[i] >= y))
                    inside ^= y * m_multiples[i] + m_constants[i] < x;

                j = i;
            }

            return inside;
        }

        return false;
    }

    public string GetName()
    {
        return m_name;
    }
}
