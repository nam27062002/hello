using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBackgrounds : MonoBehaviour 
{
	public GameObject m_initialFog;
	public GameObject m_initialSpecialFog;
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
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);
		m_initialColor = m_clouds[0].material.GetColor("_Tint");
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);	
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);
	}

	private void OnScreenChanged(MenuScreen _from, MenuScreen _to) {
		if ( _to == MenuScreen.PLAY )
		{
			IDragonData selectedDragonData = DragonManager.GetDragonData(UsersManager.currentUser.CurrentDragon);
			if ( selectedDragonData.type == IDragonData.Type.SPECIAL)
			{
				m_initialSpecialFog.SetActive(true);
				m_initialFog.SetActive(false);
			}
			else
			{
				m_initialSpecialFog.SetActive(false);
				m_initialFog.SetActive(true);
				m_background.material.SetVector( "_MoonOffset", m_hideMoonPosition );

			}
		}
		else if ( _to == MenuScreen.DRAGON_SELECTION )
		{
			IDragonData selectedDragonData = DragonManager.GetDragonData(UsersManager.currentUser.CurrentDragon);
			OnDragonSelected( selectedDragonData.type, true);
		}
        else
        {
            // Return to default color (so the clouds in other screens are white again)
            if (m_showingSpecial)
            {
                HideSpecial(false);
            }

        }
	}
	
	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// If owned and different from profile's current dragon, update profile
		// [AOC] Consider the newly selected dragon's type
		IDragonData selectedDragonData = DragonManager.GetDragonData(_sku);
		OnDragonSelected( selectedDragonData.type );
	}

	public void OnDragonSelected( IDragonData.Type _dragonType, bool _instant = false )
	{
		if( _dragonType == IDragonData.Type.SPECIAL ) 
		{
			if (!m_showingSpecial)
			{
				ShowSpecialMode(_instant);
			}
		}
		else
		{
			if ( m_showingSpecial )
			{
				HideSpecial(_instant);
			}
		}
	}

	void ShowSpecialMode( bool _instant = false)
	{
		m_showingSpecial = true;
		if ( _instant )
		{
			SetCloudsColor(m_cloudsColor);
			m_background.material.SetVector( "_MoonOffset", m_showMoonPosition );
		}
		else
		{
			StartCoroutine(CloudsToColor( m_cloudsColor ));
			StartCoroutine(MoveMoon( m_hideMoonPosition, m_showMoonPosition) );
		}
		
	}

	void HideSpecial( bool _instant = false )
	{
		m_showingSpecial = false;
		if ( _instant )
		{
			SetCloudsColor(m_initialColor);
			m_background.material.SetVector( "_MoonOffset", m_hideMoonPosition );
		}
		else
		{
			StartCoroutine(CloudsToColor( m_initialColor ));
			StartCoroutine( MoveMoon(m_showMoonPosition, m_hideMoonPosition) );
		}
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
				SetCloudsColor(c);
			}
		}
	}

	public void SetCloudsColor( Color c )
	{
		for (int i = 0; i < m_clouds.Count; i++)
		{
			m_clouds[i].material.SetColor("_Tint", c);
		}
	}

	IEnumerator MoveMoon( Vector4 startValue, Vector4 endValue )
	{
		float time = 0;
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
