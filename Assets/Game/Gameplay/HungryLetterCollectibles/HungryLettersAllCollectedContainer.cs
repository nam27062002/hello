using FGOL;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HungryLettersAllCollectedContainer : MonoBehaviour
{

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private Transform m_transform;
	private List<GameObject> m_originalChildrenList;

	private ParticleSystem m_particle;
	private Transform m_originalParent;
	private Quaternion m_originalRotation;
	private Vector3 m_originalPosition;
	private Vector3 m_originalLocalScale;

	//------------------------------------------------------------
	// Public Properties:
	//------------------------------------------------------------

	public Transform cachedTransform { get { return m_transform; } }
	// public TweenTransform tweenTransform { get { return m_tweenTransform; } }

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
		m_transform = transform;
		m_originalParent = m_transform.parent;
		m_originalPosition = m_transform.localPosition;		
		m_originalRotation = m_transform.localRotation;
		m_originalLocalScale = m_transform.localScale;

		m_originalChildrenList = new List<GameObject>();
		// cache the gameobject that don't need to be removed when resetting the panel.
		for(int i = 0; i < m_transform.childCount; i++)
		{
			GameObject go = m_transform.GetChild(i).gameObject;
			m_originalChildrenList.Add(go);
		}

		m_particle = GetComponentInChildren<ParticleSystem>();
		Assert.Fatal(m_particle != null);
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void StartAllCollectedAnimation()
	{
		// m_tweenRotation.PlayForward();
		// m_tweenTransform.PlayForward();
		DOTween.Restart(gameObject);
		m_particle.Play();
	}

	public void Reset()
	{
		// remove the eventual movers moved in the letter places.
		for(int i = 0; i < m_transform.childCount; i++)
		{
			GameObject go = m_transform.GetChild(i).gameObject;
			if(!m_originalChildrenList.Contains(go))
			{
				DestroyImmediate(go);
			}
		}
		// reset the tweens
		// m_tweenRotation.ResetToBeginning();
		// m_tweenTransform.ResetToBeginning();
		m_transform.parent = m_originalParent;
		m_transform.localPosition = m_originalPosition;
		m_transform.localRotation = m_originalRotation;
		m_transform.localScale = m_originalLocalScale;
		if(m_particle != null)
		{
			m_particle.Stop();
		}
	}

}