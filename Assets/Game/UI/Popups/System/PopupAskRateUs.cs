using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupAskRateUs : MonoBehaviour {

	public const string PATH = "UI/Popups/Message/PF_PopupAskRateUs";

	public void OnYes()
	{
		// Open rate us
#if UNITY_IOS
		// TODO: store id for this app
		DeviceUtilsManager.SharedInstance.OpenMarketForRating( Application.identifier , true);
#elif UNITY_ANDROID
		DeviceUtilsManager.SharedInstance.OpenMarketForRating();
#endif

		// Set to never ask again
		Prefs.SetBoolPlayer( Prefs.RATE_CHECK, false );
		GetComponent<PopupController>().Close(true);
	}

	public void OnLater()
	{
		int laters = Prefs.GetIntPlayer( Prefs.RATE_LATERS, 0);
		laters++;
		if ( laters < 3 )
		{
			// Wait 2 days
			System.DateTime futureTime = System.DateTime.Now.AddDays(2);
			Prefs.SetStringPlayer( Prefs.RATE_FUTURE_DATE, futureTime.ToString());
		}
		else
		{
			// Wait one year
			System.DateTime futureTime = System.DateTime.Now.AddYears(1);
			Prefs.SetStringPlayer( Prefs.RATE_FUTURE_DATE, futureTime.ToString());
		}

		Prefs.SetIntPlayer( Prefs.RATE_LATERS, laters);

		GetComponent<PopupController>().Close(true);
	}

	public void OnNeverAskAgain()
	{
		// Set to never ask again
		Prefs.SetBoolPlayer( Prefs.RATE_CHECK, false );
		GetComponent<PopupController>().Close(true);
	}
}
