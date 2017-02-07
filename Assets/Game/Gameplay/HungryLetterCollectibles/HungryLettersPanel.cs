﻿using FGOL.Events;
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class HungryLettersPanel : MonoBehaviour
{

	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	private HungryLetterUiContainer m_uiLetterContainer;
	[SerializeField]
	private float m_uiLetterLocalScale = 30f;
	[SerializeField]
	private float m_presentPanelDelay = 0f;
	[SerializeField]
	private float m_tweenLetterDelay = 0f;
	[SerializeField]
	private float m_dismissPanelDelay = 1f;
	[SerializeField]
	private HungryLettersAllCollectedPanel m_panel;
	[SerializeField]
	private float m_fromAllInPlaceToAllCollectedDelay = 1f;
	[SerializeField]
	private float m_dismissAllCollectedPanelDelay = 1f;
	[SerializeField]
	private HungryLettersAllCollectedContainer[] m_letterPlaces;

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private bool m_tweening;
	private bool m_panelInPlace;
	private bool m_presenting;
	private bool m_allCollectedLock;
	private int m_letterInPlaceCounter;
	private int m_allCollectedInPlaceCounter;
	private UnityEngine.Coroutine m_dismissCoroutine;

	//------------------------------------------------------------
	// Public Properties:
	//------------------------------------------------------------

	public static HungryLettersPanel Instance { get; private set; }

	public float tweenLetterDelay { get { return m_tweenLetterDelay; } }
	
	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
		if(Instance == null)
		{
			Instance = this;			
		}
		else if (Instance != this)
		{
			DestroyImmediate(gameObject);
		}

		// m_tween.AddOnFinished(TweenCompleted);
		m_uiLetterContainer.Init(m_letterPlaces.Length);
	}

	protected void Start()
	{
		// TODO: on editor!!
		/*
		// register to the callbacks of the all collected letter system.
		for(int i = 0; i < m_letterPlaces.Length; i++)
		{
			// m_letterPlaces[i].tweenTransform.AddOnFinished(OnAllCollectedAnimationFinished);
		}
		*/
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void TransferLetterToUi(HungryLetter letterToMove, Action moveLetterCallback = null)
	{
		// if there is a dismiss coroutine in progress, stop it.
		if(m_dismissCoroutine != null)
		{
			StopCoroutine(m_dismissCoroutine);
			m_dismissCoroutine = null;
			if(m_tweening && !m_presenting)
			{
				m_panelInPlace = false;
			}
			m_tweening = false;			
		}
		// set the letter to send to the panel.
		m_uiLetterContainer.TransferLetterToUi(letterToMove, GetLetterPlace(letterToMove.letter), m_uiLetterLocalScale, moveLetterCallback);
		if(!m_panelInPlace)
		{
			// safety assignment.
			m_dismissCoroutine = null;
			if(m_presentPanelDelay > 0f)
			{
				StartCoroutine(Delay(m_presentPanelDelay, Present));
			}
			else
			{
				Present();
			}
		}
		else
		{
			m_uiLetterContainer.TweenLetter();
		}
    }

	public void RequestDismission()
	{
		if(!m_allCollectedLock)
		{
			if(m_dismissPanelDelay > 0f)
			{
				m_dismissCoroutine = StartCoroutine(Delay(m_dismissPanelDelay, Dismiss));
			}
			else
			{
				Dismiss();
			}
		}
	}

	public void TweenCompleted()
	{
		m_panelInPlace = m_presenting;
		m_tweening = false;
		if(m_panelInPlace)
		{
			m_uiLetterContainer.TweenLetter();
		}
	}

	public void AllCollected()
	{
		m_allCollectedLock = true;
	}

	public void OnLetterInPlace()
	{
		m_letterInPlaceCounter++;
		// when all the letters are in place on the panel trigger the all collected animation.
		if(m_letterInPlaceCounter == m_letterPlaces.Length)
		{
			// dismiss the all collected panel.
			if(m_fromAllInPlaceToAllCollectedDelay > 0f)
			{
				m_dismissCoroutine = StartCoroutine(Delay(m_fromAllInPlaceToAllCollectedDelay, StartAllCollectedAnimation));
			}
			else
			{
				StartAllCollectedAnimation();
			}
		}
	}

	public void ReInit(int lettersNumber)
	{
		// re initialize the all collected animators.
		m_allCollectedLock = false;
		m_letterInPlaceCounter = m_allCollectedInPlaceCounter = 0;
		for(int i = 0; i < m_letterPlaces.Length; i++)
		{
			m_letterPlaces[i].Reset();
		}
		// re initialize the container.
		m_uiLetterContainer.ReInit(lettersNumber);
		// reset the all collected panel.
		m_panel.Reset();
	}

	//------------------------------------------------------------
	// Private Methods:
	//------------------------------------------------------------

	private void Present()
	{
		if(m_tweening)
		{
			return;
		}

		m_tweening = true;
		m_presenting = true;
		DOTween.Restart( gameObject);
		// m_tween.PlayForward();
	}

	private void Dismiss()
	{
		if(m_tweening)
		{
			return;
		}

		m_tweening = true;
		m_presenting = false;
		DOTween.PlayBackwards(gameObject);
		TweenCompleted();
		// m_tween.PlayReverse();
	}

	private Transform GetLetterPlace(HungryLettersManager.CollectibleLetters letter)
	{
		return m_letterPlaces[(int)letter].cachedTransform;
	}

	private void StartAllCollectedAnimation()
	{
		for(int i = 0; i < m_letterPlaces.Length; i++)
		{
			m_letterPlaces[i].StartAllCollectedAnimation();
		}
	}

	private void OnAllCollectedAnimationFinished()
	{
		m_allCollectedInPlaceCounter++;
		if(m_allCollectedInPlaceCounter == m_letterPlaces.Length)
		{
			// trigger what's needed.
			// TODO Recover situational text!
			// TextSystem.Instance.ShowSituationalText(SituationalTextSystem.Type.HungryAllLettersCollected);
			// AudioManager.PlaySfx(AudioManager.Ui.HungryComplete);
			AudioController.Play("AudioManager.Ui.HungryComplete");
			// send out the event for all the hungry letters being collected.
			Messenger.Broadcast(GameEvents.ALL_HUNGRY_LETTERS_COLLECTED);
			// release the lock and dismiss this panel.
			m_allCollectedLock = false;
			RequestDismission();
			// dismiss the all collected panel.
			if(m_dismissAllCollectedPanelDelay > 0f)
			{
				m_dismissCoroutine = StartCoroutine(Delay(m_dismissAllCollectedPanelDelay, m_panel.Dismiss));
			}
			else
			{
				m_panel.Dismiss();
			}
		}
	}

	System.Collections.IEnumerator Delay( float seconds, Action action)
	{
		yield return new WaitForSeconds( seconds);
		action();
	}

}