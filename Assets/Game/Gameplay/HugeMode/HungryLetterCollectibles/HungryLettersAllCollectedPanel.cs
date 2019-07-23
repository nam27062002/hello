using FGOL;
using UnityEngine;
using DG.Tweening;

public class HungryLettersAllCollectedPanel : MonoBehaviour
{

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private DOTweenAnimation m_tween;

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	public void Awake()
	{
		m_tween = GetComponent<DOTweenAnimation>();
		Assert.Fatal(m_tween != null);
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void Dismiss()
	{
		m_tween.DORestart();
	}

	public void Reset()
	{
		m_tween.DOPlayBackwards();
	}

}