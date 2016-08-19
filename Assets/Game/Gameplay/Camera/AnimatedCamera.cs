using UnityEngine;
using System.Collections;

public class AnimatedCamera : MonoBehaviour 
{

	public Animator m_animator;
	public Camera m_canera;


	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	public void PlayIntro()
	{
		m_canera.enabled = true;
		m_animator.Play("Intro", 0, 0);
	}
}
