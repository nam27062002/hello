﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDragonBirdControl : MonoBehaviour {

	public GameObject m_prefab;
	public List<Material> m_materials = new List<Material>();
	private GameObject m_prefabInstance;
	private Animator m_birdAnimator;
	private Renderer m_birdRenderer;
	private Animator m_dragonAnimator;
	private bool m_playingBird;
	private bool m_waitToSync;

	private static int MENU_ALT_1 = Animator.StringToHash("MenuAlt1");
	private static int SELECTION_SCREEN = Animator.StringToHash("SelectionScreen");

	// Use this for initialization
	void Awake () {
		Transform view = transform.Find("view");
		m_dragonAnimator = view.GetComponent<Animator>();

		m_prefabInstance = Instantiate<GameObject>(m_prefab);
		m_prefabInstance.transform.parent = view;
		m_prefabInstance.transform.localPosition = Vector3.zero;
		m_prefabInstance.transform.localRotation = Quaternion.identity;
		m_birdAnimator = m_prefabInstance.GetComponent<Animator>();
		m_birdRenderer = m_prefabInstance.GetComponentInChildren<Renderer>();
		m_prefabInstance.SetActive(false);
		m_playingBird = false;
	}

	public void PlayBird()
	{	
		m_playingBird = true;
		m_waitToSync = true;
		m_prefabInstance.SetActive(true);
		m_birdAnimator.speed = 0.9f;
		m_birdAnimator.Play("SelectionScreen", 0, 0);
		m_birdRenderer.material = m_materials[ Random.Range(0, m_materials.Count) ];
	}

	void Update()
	{
		if ( m_playingBird )
		{
			if ( m_waitToSync )
			{
				AnimatorStateInfo dragonStateInfo = m_dragonAnimator.GetCurrentAnimatorStateInfo(0);
				if ( dragonStateInfo.shortNameHash == MENU_ALT_1 )
				{
					AnimatorStateInfo birdStateInfo = m_birdAnimator.GetCurrentAnimatorStateInfo(0);

					float diff = dragonStateInfo.normalizedTime - birdStateInfo.normalizedTime;

					if ( Mathf.Abs(diff) > 0.01 )
					{
						// Try To adjust
						m_birdAnimator.speed = 1 + diff * 3;
					}
					else
					{
						m_birdAnimator.speed = 1.0f;
						m_birdAnimator.Play(SELECTION_SCREEN, 0, dragonStateInfo.normalizedTime);
						m_waitToSync = false;
					}

					/*
					if ( Mathf.Abs(birdStateInfo.normalizedTime - dragonStateInfo.normalizedTime) > Mathf.Epsilon)
					{
						
						float norm = Mathf.Lerp( birdStateInfo.normalizedTime, dragonStateInfo.normalizedTime, Time.deltaTime * 10);
						m_birdAnimator.Play(SELECTION_SCREEN, 0, norm);
					}
					else
					{
						
					}
					*/
				}
			}
			else
			{
				AnimatorStateInfo birdStateInfo = m_birdAnimator.GetCurrentAnimatorStateInfo(0);
				if (birdStateInfo.shortNameHash != SELECTION_SCREEN )
				{
					m_prefabInstance.SetActive(false);
					m_playingBird = false;
				}
			}
		}
	}
}
