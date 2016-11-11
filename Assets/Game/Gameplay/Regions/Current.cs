using Assets.Code.Game.Spline;
using System;
using UnityEngine;

[Serializable]
public class Current : Region
{
	[SerializeField]
	public BezierSplineForce splineForce;

    public bool m_hideSituationalText;

    [SerializeField]
    public bool m_playEnterSFX = true;

	public Current(string name, float minX, float maxX, float minY, float maxY, float[] vertexYs, float[] multiples, float[] constants, BezierSplineForce splineForce)
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
		this.splineForce = splineForce;
	}

	public bool IsInCurrentDirection(GameObject gameObject)
	{
		if (splineForce == null) 
			return false;
		
		return splineForce.IsInCurrentDirection(gameObject);
	}

    public bool ShouldPlayEnterSFX( )
    {
        return m_playEnterSFX;
    }
}