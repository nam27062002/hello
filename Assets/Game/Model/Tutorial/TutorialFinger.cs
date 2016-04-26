using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialFinger : MonoBehaviour 
{

	public const string PATH = "UI/Popups/Tutorial/PF_TutorialFinger";
	
	private Sequence m_sequence = null;
	private Transform m_start;
	private Transform m_end;
	private Image m_image;
	private RectTransform m_rectTransform;
	private float m_moveSpeed = 100;

	void Awake () 
	{
		m_image = GetComponent<Image>();
		m_rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void SetupDrag( Transform _start, Transform _end )
	{
		ClearSequence();
		m_start = _start;
		m_end = _end;

		float moveDuration = (_start.position-_end.position).magnitude / 1;

		// Create sequence, pause it
		m_sequence = DOTween.Sequence()
			.OnStart(OnStartSequence)

			// In
			.Append(m_rectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack))
			.Join(m_image.DOFade(1f, 0.5f))

			// Wait
			.AppendInterval(0.2f)

			// Move
			.Append( m_rectTransform.DOMove( m_end.position, moveDuration))

			// Wait
			.AppendInterval(0.2f)

			// Out
			.Append( m_rectTransform.DOScale( 2, 0.5f))
			.Join( m_image.DOFade(0f, 0.5f))

			// Sequence Loop
			.OnComplete( ResetDrag);
			// .SetLoops(-1, LoopType.Restart);
	}

	void ResetDrag()
	{
		SetupDrag( m_start, m_end);
	}

	void OnStartSequence()
	{
		// Double scale
		m_rectTransform.localScale = Vector3.one * 2;
		// Set start sequence position
		transform.position = m_start.position;
		// Set transparent
		Color c = Color.white;
		c.a = 0;
		m_image.color = c;
	}

	/// <summary>
	/// Kill and destroy the current sequence, if any.
	/// </summary>
	private void ClearSequence() {
		// Just in case
		if(m_sequence != null) {
			m_sequence.Kill();
			m_sequence = null;
		}
	}


	public void Show( bool instant = false )
	{
		// m_sequence.Play();
	}

	public void Hide( bool instant = false )
	{
		
	}

}
