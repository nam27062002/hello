using UnityEngine;
using System.Collections;

public class AnimStateSetups : MonoBehaviour {

	public Range m_timeToGlide = new Range(3f, 4f);
	public Range m_glidingTime = new Range(4f, 6f);

	// Use this for initialization
	void Start () 
	{
		Animator anim = GetComponent<Animator>();
		if ( anim != null )
		{
			FlyLoopBehaviour loopBehaviour = anim.GetBehaviour<FlyLoopBehaviour>();
			if ( loopBehaviour != null )
			{
				loopBehaviour.m_timeToGlide = m_timeToGlide;
				loopBehaviour.ResetTimer();
			}

			GlideBehaviour glide = anim.GetBehaviour<GlideBehaviour>();
			if (glide != null)
			{
				glide.m_glidingTime = m_glidingTime;
				glide.ResetTimer();
			}
		}
	}
	

}
