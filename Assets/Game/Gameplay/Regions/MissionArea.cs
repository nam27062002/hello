using System;

[Serializable]
public class MissionArea : Region
{
    public MissionArea(string name, float minX, float maxX, float minY, float maxY, float[] vertexYs, float[] multiples, float[] constants)
	{
        m_name = name;
		m_minX = minX;
		m_maxX = maxX;
		m_minY = minY;
		m_maxY = maxY;
		m_vertexAmount = vertexYs.Length;
		m_vertexYs = vertexYs;
		m_multiples = multiples;
		m_constants = constants;
	}
}