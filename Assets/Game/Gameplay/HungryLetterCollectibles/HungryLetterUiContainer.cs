using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class HungryLetterUiContainer : MonoBehaviour
{


	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	private GameObject m_letterMoverPrefab;

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private List<Transform> m_letterMovers;
	private List<Transform> m_letterMoversQueue;

	private Action m_moveLetterCallback;

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void Init(int lettersNumber)
	{
		m_letterMovers = new List<Transform>();
		m_letterMoversQueue = new List<Transform>();
		for(int i = 0; i < lettersNumber; i++)
		{
			GameObject go = Instantiate(m_letterMoverPrefab) as GameObject;
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = Vector3.zero;
			m_letterMovers.Add(go.transform);
		}
	}

	public void TransferLetterToUi(HungryLetter letterToMove, Transform to, float guiScale, Action moveLetterCallback)
	{
		m_moveLetterCallback = moveLetterCallback;
		letterToMove.ChangeLayers();
		Transform mover = m_letterMovers[0];
		m_letterMovers.Remove(mover);
		letterToMove.cachedTransform.parent = mover;
		letterToMove.cachedTransform.localPosition = Vector3.zero;
		letterToMove.cachedTransform.localScale = new Vector3(guiScale, guiScale, guiScale);
		// reset the scale of the mover.
		mover.localScale = Vector3.zero;
		DOTween.Restart( mover.gameObject, "scale");
		DOTween.Restart( mover.gameObject, "position");
		DOTween.Restart( mover.gameObject, "rotation");
		// DOTween.Play( mover, "scale");
		// DOTween.Play( mover, "position");
		// DOTween.Play( mover, "rotation");

		// mover.m_tweenScale.ResetToBeginning();
		// mover.m_tweenPosition.ResetToBeginning();
		// mover.m_tweenRotation.ResetToBeginning();
		// trigger the scale.
		// mover.m_tweenScale.PlayForward();
		// mover.m_tweenPosition.PlayForward();
		// mover.m_tweenRotation.PlayForward();
		// configure the tween transform.

		DOTweenAnimation[] anims = mover.GetComponents<DOTweenAnimation>();
		for( int i = 0; i<anims.Length; i++ )
		{
			if (anims[i].id == "transform")
			{
				switch( anims[i].animationType )
				{
					case DG.Tweening.Core.DOTweenAnimationType.Move:
					{
						anims[i].endValueTransform = to;
						anims[i].targetType = DG.Tweening.Core.TargetType.Transform;
						anims[i].onComplete.RemoveAllListeners();
						anims[i].onComplete.AddListener(OnTweenCompleted);
					}break;
					case DG.Tweening.Core.DOTweenAnimationType.Rotate:
					{
						anims[i].endValueTransform = to;
						anims[i].targetType = DG.Tweening.Core.TargetType.Transform;
					}break;
					case DG.Tweening.Core.DOTweenAnimationType.Scale:
					{
						anims[i].endValueTransform = to;
						anims[i].targetType = DG.Tweening.Core.TargetType.Transform;
					}break;
				}
			}
		}

		// mover.m_tweenTransform.to = to;
		// mover.m_tweenTransform.AddOnFinished(OnTweenCompleted);
		// add this mover to the queue of the ones that need to move in the ui.
		m_letterMoversQueue.Add(mover);
	}

	public void TweenLetter()
	{
		// get the first letter to move.
		Transform mover = m_letterMoversQueue[0];
		// move the letter.
		if(HungryLettersPanel.Instance.tweenLetterDelay > 0f)
		{
			StartCoroutine(MoveLetterCoroutine(mover));
		}
		else
		{
			MoveLetter(mover);
		}
		// remove the letter from the queue.
		m_letterMoversQueue.Remove(mover);
	}

	public void ReInit(int lettersNumber)
	{
		Init(lettersNumber);
	}

	//------------------------------------------------------------
	// Event Callbacks:
	//------------------------------------------------------------

	public void OnTweenCompleted()
	{
		// notify if necessary.
		if(m_moveLetterCallback != null)
		{
			m_moveLetterCallback();
        }
		// notify the panel that the letter is in place.
		HungryLettersPanel.Instance.OnLetterInPlace();

		// check if there are more letters to tween.
		if(m_letterMoversQueue.Count > 0)
		{
			TweenLetter();
		}
		else
		{
			// dismiss the panel.
			HungryLettersPanel.Instance.RequestDismission();
		}
	}

	//------------------------------------------------------------
	// Private Methods:
	//------------------------------------------------------------

	private IEnumerator MoveLetterCoroutine(Transform mover)
	{
		yield return new WaitForSeconds(HungryLettersPanel.Instance.tweenLetterDelay);
		MoveLetter(mover);
	}

	private void MoveLetter(Transform mover)
	{
		DOTween.Restart( mover.gameObject, "transform");
	}

}