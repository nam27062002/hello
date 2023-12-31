﻿using UnityEngine;
using System.Collections;

public class AnimatedCamera : MonoBehaviour 
{

	public Animator m_animator;
	public Camera m_canera;


	// Use this for initialization
	void Start () 
	{
		Messenger.AddListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
		Messenger.AddListener(MessengerEvents.CAMERA_INTRO_DONE, IntroDone);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.GAME_COUNTDOWN_ENDED, CountDownEnded);
		Messenger.RemoveListener(MessengerEvents.CAMERA_INTRO_DONE, IntroDone);
	}

	public void CountDownEnded()
	{
		// m_canera.enabled = false;
	}

	public void IntroDone()
	{
		m_canera.enabled = false;
	}

	// Update is called once per frame
	public void PlayIntro()
	{
		m_canera.enabled = true;
		m_animator.Play("Intro", 0, 0);
	}
}
