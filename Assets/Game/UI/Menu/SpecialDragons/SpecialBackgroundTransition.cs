using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBackgroundTransition : MonoBehaviour 
{
	bool m_showingSpecial = false;

	public Color m_cloudsColor = Color.grey;
	protected Color m_initialColor;
	public List< Renderer > m_clouds = new List<Renderer>();
	public Renderer m_background;
	public float m_transitionDuration = 0.25f;
	public Vector4 m_showMoonPosition;
	public Vector4 m_hideMoonPosition;

	void Awake () 
	{
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);	
		m_initialColor = m_clouds[0].material.GetColor("_Tint");
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);	
	}
	
	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// If owned and different from profile's current dragon, update profile
		// [AOC] Consider the newly selected dragon's type
		IDragonData selectedDragonData = DragonManager.GetDragonData(_sku);
		if( selectedDragonData.type == IDragonData.Type.SPECIAL ) 
		{
			if (!m_showingSpecial)
			{
				ShowSpecialMode();
			}
		}
		else
		{
			if ( m_showingSpecial )
			{
				HideSpecial();
			}
		}
	}

	void ShowSpecialMode()
	{
		m_showingSpecial = true;
		StartCoroutine(CloudsToColor( m_cloudsColor ));
		StartCoroutine( MoveMoonTo(m_showMoonPosition) );
	}

	void HideSpecial()
	{
		m_showingSpecial = false;
		StartCoroutine(CloudsToColor( m_initialColor ));
		StartCoroutine( MoveMoonTo(m_hideMoonPosition) );
	}

	IEnumerator CloudsToColor( Color _color )
	{
		if ( m_clouds.Count > 0 )
		{
			float time = 0;
			Color c = m_clouds[0].material.GetColor( "_Tint" );
			Color startC = c;
			while( time < m_transitionDuration)
			{
				yield return null;	
				time += Time.deltaTime;
				c = Color.Lerp(startC, _color, time / m_transitionDuration);
				for (int i = 0; i < m_clouds.Count; i++)
				{
					m_clouds[i].material.SetColor("_Tint", c);
				}
			}
		}
	}

	IEnumerator MoveMoonTo( Vector4 endValue )
	{
		float time = 0;
		Vector4 startValue = m_background.material.GetVector("_MoonOffset");
		Vector4 value = startValue;
		while( time < m_transitionDuration)
		{
			yield return null;
			time += Time.deltaTime;
			value = Vector4.Lerp( startValue, endValue, time / m_transitionDuration);
			m_background.material.SetVector( "_MoonOffset", value );
		}
	}
}
