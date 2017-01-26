using UnityEngine;
using System.Collections;

public class DrunkCameraEffect : MonoBehaviour 
{	
	public Material m_material = null;
    public AnimationCurve curve;
    public float m_timeEffect = 1.0f;

	private bool m_DrunkEffect = false;
    private bool m_isDrunk = false;
    private float m_startTime = 0.0f;

	void Awake()
	{
	}

    private void Start()
    {
        setDrunk(true);
    }

	private void OnDestroy() 
	{
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if (m_DrunkEffect)
        {
            float delay = Time.time - m_startTime / m_timeEffect;
            float intensity;


            if (m_isDrunk)
            {
                intensity = (delay > 1.0f) ? 1.0f: curve.Evaluate(delay);
            }
            else
            {
                intensity = 1.0f - delay;
                if (intensity < 0.0f)
                {
                    intensity = 0.0f;
                    m_DrunkEffect = false;
                }
            }

            m_material.SetFloat("_Intensity", intensity);
            Graphics.Blit(source, destination, m_material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void setDrunk(bool value)
    {
        m_startTime = Time.time;
        m_isDrunk = value;
        if (value)
        {
            m_DrunkEffect = true;
        }
    }
}
