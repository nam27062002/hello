using FGOL.Events;
using System;
using UnityEngine;

public class HungryLetterMapMarker : MapMarker {

	private HungryLetter m_hungryLetter;
	public GameObject m_mapLetter;

	override protected void Awake( )
	{
		base.Awake();
		m_hungryLetter = transform.parent.GetComponent<HungryLetter>();
		Messenger.AddListener<Reward>(GameEvents.LETTER_COLLECTED, OnCollectedHungryLetter);
	}

	override protected void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener<Reward>(GameEvents.LETTER_COLLECTED, OnCollectedHungryLetter);
	}

	public void OnUpdateMarkerStatus()
	{
		bool showMarker = true;
		bool isLetterCollected = m_hungryLetter.GetHungryLettersManager ().IsLetterCollected ( m_hungryLetter.letter );
		if ( isLetterCollected )
			showMarker = false;
		if ( m_mapLetter != null )
			m_mapLetter.SetActive( showMarker );
	}

	private void OnCollectedHungryLetter( Reward _r )
	{
		OnUpdateMarkerStatus();
	}
}